using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RevitReactionImporter
{
    public enum AnnotationType
    {
        None,
        Reaction,
        StudCount,
        Camber,
        Size
    }

    public class VisualizationHistory
    {
        public List<BeamVisualizationStatus> BeamVisualizationStatuses { get; set; }
        public class BeamVisualizationStatus
        {
            public int BeamId { get; set; }
            [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
            public VisualizationStatus Reactions { get; set; }
            public VisualizationStatus Studcount { get; set; }
            public VisualizationStatus CamberSize { get; set; }
            public VisualizationStatus BeamSize { get; set; }

        }

        public VisualizationHistory()
        {
            BeamVisualizationStatuses = new List<BeamVisualizationStatus>();
        }
        public VisualizationHistory(List<int> beamIds)
        {
            var beamVisualizationStatuses = new List<BeamVisualizationStatus>();
            for (int i = 0; i < beamIds.Count; i++)
            {
                var bvs = new BeamVisualizationStatus();
                bvs.BeamId = beamIds[i];
                bvs.BeamSize = VisualizationStatus.Unmapped;
                bvs.Reactions = VisualizationStatus.Unmapped;
                bvs.Studcount = VisualizationStatus.Unmapped;
                bvs.CamberSize = VisualizationStatus.Unmapped;
                beamVisualizationStatuses.Add(bvs);
            }
            this.BeamVisualizationStatuses = beamVisualizationStatuses;

        }
    }



    public class ResultsAnnotator
    {
        private Document _document = null;
        private ModelCompare.Results Results { get; set; }
        public int StartReactionTagId { get; set; }
        private int MiddleReactionTagId { get; set; }
        private int EndReactionTagId { get; set; }

        public ResultsAnnotator(Document document, ModelCompare.Results results, AnnotationType annotationType)
        {
            Results = results;

            _document = document;
            
        }

        // TAGGING
        private static Family FindFamilyByName(Document doc, string familyName)
        {
            FilteredElementCollector familyCollector
              = new FilteredElementCollector(doc)
                .OfClass(typeof(Family));

            Family family = familyCollector.FirstOrDefault<Element>(
              e => e.Name.Equals(familyName))
                as Family;
            return family;
        }

        private void TryToGetReactionTagFamilies(Document document)
        {
            string startReactionTagFileName = "SAMiro - Start Reaction Tag";
            string endReactionTagFileName = "SAMiro - End Reaction Tag";
            string middleReactionTagFileName = "SAMiro - Middle Reaction Tag";

            var startReactionTag = FindFamilyByName(document, startReactionTagFileName);
            var endReactionTag = FindFamilyByName(document, endReactionTagFileName);
            var middleReactionTag = FindFamilyByName(document, middleReactionTagFileName);
            LoadUnloadedFamilyFromFile(startReactionTag, document, startReactionTagFileName);
            LoadUnloadedFamilyFromFile(endReactionTag, document, endReactionTagFileName);
            LoadUnloadedFamilyFromFile(middleReactionTag, document, middleReactionTagFileName);

            StartReactionTagId = GetFamilySymbolId(document, BuiltInCategory.OST_StructuralFramingTags, startReactionTagFileName);
            MiddleReactionTagId = GetFamilySymbolId(document, BuiltInCategory.OST_StructuralFramingTags, middleReactionTagFileName);
            EndReactionTagId = GetFamilySymbolId(document, BuiltInCategory.OST_StructuralFramingTags, endReactionTagFileName);

        }

        private void LoadUnloadedFamilyFromFile(Family family, Document document, string fileName)
        {
            string familyPath = @"Z:\Admin\3. Drawing Production\Revit\MIRO STANDARDS (PROGRESS)\Miro Library_restored\Miro Library\Annotation\Tags\Beam Tags\" + fileName + ".rfa";
            if (family == null)
            {
                // It is not present, so check for 
                // the file to load it from:
                if (!File.Exists(familyPath))
                {
                    System.Windows.Forms.MessageBox.Show("Could not find the reaction tag family. Please ensure that the " + family.Name + " family file exists in " + familyPath);
                    return;
                }

                // Load family from file:
                var loadFamilyTransaction = new Transaction(document, "Load Family");
                loadFamilyTransaction.Start();
                document.LoadFamily(familyPath, out family);
                loadFamilyTransaction.Commit();
            }

        }

        private void AddReactionTagsSingleBeam(Document document, FamilyInstance revitBeamInstance, Beam revitBeam)
        {

            double reactionLimit = 14.0; // TODO: un hardcode.
            double startReaction = Double.Parse(revitBeam.StartReactionTotal);
            double endReaction = Double.Parse(revitBeam.EndReactionTotal);
            if(startReaction<= reactionLimit && endReaction <= reactionLimit)
            {
                return;
            }
            Autodesk.Revit.Creation.Document createDoc = document.Create;
            // Get the start & end reaction values.

            // Check if the reactions are equal.
            bool areReactionsEqual = AreReactionsEqual(startReaction, endReaction);
            if(areReactionsEqual)
            {
                double[] offsetAwayFromBeam = DetermineOffsetAwayFromBeam(revitBeam);
                double locX = ((revitBeam.StartPoint[0] / 12.0) + (revitBeam.EndPoint[0] / 12.0)) / 2.0;
                double locY = ((revitBeam.StartPoint[1] / 12.0) + (revitBeam.EndPoint[1] / 12.0)) / 2.0;
                double locZ = ((revitBeam.StartPoint[2] / 12.0) + (revitBeam.EndPoint[2] / 12.0)) / 2.0;
                locX += offsetAwayFromBeam[0];
                locY += offsetAwayFromBeam[1];

                var location = new XYZ(locX, locY, locZ);
                IndependentTag tag = createDoc.NewTag(document.ActiveView, revitBeamInstance, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, location);
                tag.ChangeTypeId(new ElementId(MiddleReactionTagId));
            }
            else
            {
                if (startReaction > reactionLimit)
                {
                    AddStartReactionTag(revitBeam, document, revitBeamInstance, createDoc);
                }
                if (endReaction > reactionLimit)
                {
                    AddEndReactionTag(revitBeam, document, revitBeamInstance, createDoc);
                }

            }
        }

        private double DetermineInwardOffsetDirection(double start, double end)
        {
            if (start > end)
                return -1.0;
            return 1.0;
        }

        private void AddStartReactionTag(Beam revitBeam, Document document, FamilyInstance revitBeamInstance, Autodesk.Revit.Creation.Document createDoc)
        {
            double[] offsetFactorsInwards = DetermineOffsetInwards(revitBeam);
            
            double[] offsetAwayFromBeam = DetermineOffsetAwayFromBeam(revitBeam);
            double startX = (revitBeam.StartPoint[0] / 12.0);
            double startY = (revitBeam.StartPoint[1] / 12.0);
            double endX = (revitBeam.EndPoint[0] / 12.0);
            double endY = (revitBeam.EndPoint[1] / 12.0);

            double locX = (startX+endX) / 2.0;
            double locY = (startY+endY) / 2.0;
            double locZ = ((revitBeam.StartPoint[2] / 12.0) + (revitBeam.EndPoint[2] / 12.0)) / 2.0;
            locX = locX + offsetAwayFromBeam[0] + (offsetFactorsInwards[0] * DetermineInwardOffsetDirection(startX, endX));
            locY = locY + offsetAwayFromBeam[1] + (offsetFactorsInwards[1] * DetermineInwardOffsetDirection(startY, endY));

            var startLocation = new XYZ(locX, locY, locZ);
            IndependentTag startTag = createDoc.NewTag(document.ActiveView, revitBeamInstance, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startLocation);
            startTag.ChangeTypeId(new ElementId(StartReactionTagId));

        }
        static int GetFamilySymbolId(Document doc, BuiltInCategory bic, string tagName)
        {
            FilteredElementCollector collector = GetFamilySymbols(doc, bic);
            var collectorElements = collector.ToElements();
            var collectorList = collectorElements.ToList();
            var target = collectorList.ConvertAll(x => (FamilySymbol)x);
            FamilySymbol familySymbol = target.FirstOrDefault<FamilySymbol>(e => e.FamilyName.Equals(tagName))
                as FamilySymbol;
            return familySymbol.Id.IntegerValue;
        }

        static FilteredElementCollector GetFamilySymbols(Document doc, BuiltInCategory bic)
        {
            return GetElementsOfType(doc,
              typeof(FamilySymbol), bic);
        }
        static FilteredElementCollector GetElementsOfType(Document doc,Type type, BuiltInCategory bic)
        {
            FilteredElementCollector collector
              = new FilteredElementCollector(doc);

            collector.OfCategory(bic);
            collector.OfClass(type);

            return collector;
        }

        private void AddEndReactionTag(Beam revitBeam, Document document, FamilyInstance revitBeamInstance, Autodesk.Revit.Creation.Document createDoc)
        {
            double[] offsetFactorsInwards = DetermineOffsetInwards(revitBeam);
            double[] offsetAwayFromBeam = DetermineOffsetAwayFromBeam(revitBeam);

            double startX = (revitBeam.StartPoint[0] / 12.0);
            double startY = (revitBeam.StartPoint[1] / 12.0);
            double endX = (revitBeam.EndPoint[0] / 12.0);
            double endY = (revitBeam.EndPoint[1] / 12.0);

            double locX = ((startX + endX) / 2.0) + offsetAwayFromBeam[0] - (offsetFactorsInwards[0]*DetermineInwardOffsetDirection(startX, endX));
            double locY = ((startY + endY) / 2.0)  + offsetAwayFromBeam[1] - (offsetFactorsInwards[1] * DetermineInwardOffsetDirection(startY, endY));
            double locZ = ((revitBeam.StartPoint[2] / 12.0) + (revitBeam.EndPoint[2] / 12.0)) / 2.0;

            var location = new XYZ(locX, locY, locZ);
            IndependentTag endTag = createDoc.NewTag(document.ActiveView, revitBeamInstance, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, location);
            endTag.ChangeTypeId(new ElementId(EndReactionTagId));

        }

        private static double GetBeamOrientationRelativeToXAxis(Beam revitBeam)
        {
            double angle = 0.0;
            double startX = revitBeam.StartPoint[0] / 12.0;
            double startY = revitBeam.StartPoint[1] / 12.0;
            double endX = revitBeam.EndPoint[0] / 12.0;
            double endY = revitBeam.EndPoint[1] / 12.0;
            if(Math.Abs(endX-startX) < 0.1)
            {
                angle = Math.PI/ 2.0;
            }
            else
            {
                double slope = (endY - startY) / (endX - startX);
                angle = Math.Atan(slope); // radians.
            }
            return angle;
        }

        private static double[] DetermineOffsetAwayFromBeam(Beam revitBeam)
        {
            double tagHeightOffset = 1.0;
            double angle = GetBeamOrientationRelativeToXAxis(revitBeam);
            double[] offsets = new double[2];
            offsets[0] = Math.Sin(angle) * tagHeightOffset;
            offsets[1] = -1.0 * Math.Cos(angle) * tagHeightOffset;
            return offsets;
        }

        private static double[] DetermineOffsetInwards(Beam revitBeam)
        {
            double tagWidthOffset = 1.5;
            double angle = GetBeamOrientationRelativeToXAxis(revitBeam);
            double[] offsets = new double[2];
            offsets[0] = Math.Cos(angle) * tagWidthOffset;
            offsets[1] = Math.Sin(angle) * tagWidthOffset;
            return offsets;
        }

        private static bool AreReactionsEqual(double startReaxtion, double endReaction)
        {
            if (Math.Abs(startReaxtion - endReaction) <= 0.1)
                return true;
            return false;
        }

        public void AddReactionTags(Document document, ModelCompare.Results results)
        {
            TryToGetReactionTagFamilies(document);
            var revitBeamToRAMBeamMappingDict = results.RevitBeamToRAMBeamMapping;
            var addReactionTagsTransaction = new Transaction(document, "Add Beam Reaction Tags");
            addReactionTagsTransaction.Start();
            foreach (var revitBeam in revitBeamToRAMBeamMappingDict.Keys)
            {
                ElementId beamId = new ElementId(revitBeam.ElementId);
                var revitBeamInstance = document.GetElement(beamId) as FamilyInstance;
                AddReactionTagsSingleBeam(document, revitBeamInstance, revitBeam);
            }
            addReactionTagsTransaction.Commit();
        }


        public static void FillInBeamParameterValues(Document document, ModelCompare.Results results, AnnotationType annotationType, VisualizationHistory visualizationHistory, bool beamReactionsImported)
        {
            if(annotationType == AnnotationType.Reaction)
            {
                AssignReactions(document, results, visualizationHistory, beamReactionsImported);
            }
            else if (annotationType == AnnotationType.StudCount)
            {
                AnnotateStudCounts(document, results, visualizationHistory);
            }
            else if (annotationType == AnnotationType.Camber)
            {
                AnnotateCamberValues(document, results, visualizationHistory);
            }
            else if (annotationType == AnnotationType.Size)
            {
                return;
                // TODO:
                //ChangeSize(document, results);
            }
            else
            {
                throw new Exception("Need to debug: No Annotation Type has been defined.");
            }
        }

        public static void AssignReactions(Document document, ModelCompare.Results results, VisualizationHistory visualizationHistory, bool beamReactionsImported)
        {
            var beamIds = new List<int>();
            var annotationType = AnnotationType.Reaction;
            var revitBeamToRAMBeamMappingDict = results.RevitBeamToRAMBeamMapping;
            var annotateReactionsTransaction = new Transaction(document, "Annotate Beam Reactions");
            annotateReactionsTransaction.Start();
            foreach (var revitBeam in revitBeamToRAMBeamMappingDict.Keys) // loops through mapped beams.
            {
                revitBeam.StartReactionTotal = revitBeamToRAMBeamMappingDict[revitBeam].StartTotalReactionPositive.ToString();
                revitBeam.EndReactionTotal = revitBeamToRAMBeamMappingDict[revitBeam].EndTotalReactionPositive.ToString();
                ElementId beamId = new ElementId(revitBeam.ElementId);
                var revitBeamInstance = document.GetElement(beamId) as FamilyInstance;
                var startReactionTotalParameter = revitBeamInstance.LookupParameter("Start Reaction - Total");
                var endReactionTotalParameter = revitBeamInstance.LookupParameter("End Reaction - Total");
                startReactionTotalParameter.SetValueString(revitBeam.StartReactionTotal);
                endReactionTotalParameter.SetValueString(revitBeam.EndReactionTotal);
                beamIds.Add(revitBeam.ElementId);
            }
            annotateReactionsTransaction.Commit();
            UpdateVisualizationHistoryWithMappedBeams(beamIds, visualizationHistory, annotationType);
        }

        private static void UpdateVisualizationHistoryWithMappedBeams(List<int> beamIds, VisualizationHistory visualizationHistory, AnnotationType annotationType)
        {
            var beamVizStatuses = visualizationHistory.BeamVisualizationStatuses;
            foreach(var beamId in beamIds)
            {
                var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                if (annotationType == AnnotationType.Reaction)
                {
                    beamVizStatus.Reactions = VisualizationStatus.Mapped;
                    continue;
                }
                if (annotationType == AnnotationType.StudCount)
                {
                    beamVizStatus.Studcount = VisualizationStatus.Mapped;
                    continue;
                }
                if (annotationType == AnnotationType.Camber)
                {
                    beamVizStatus.CamberSize = VisualizationStatus.Mapped;
                    continue;
                }
                if (annotationType == AnnotationType.Size)
                {
                    beamVizStatus.BeamSize = VisualizationStatus.Mapped;
                    continue;
                }
                //else throw new Exception("Need to debug: Annotation type not recognized.");
            }

            

        }


        public static void AnnotateStudCounts(Document document, ModelCompare.Results results, VisualizationHistory visualizationHistory)
        {
            var annotationType = AnnotationType.StudCount;
            var beamIds = new List<int>();
            var revitBeamToRAMBeamMappingDict = results.RevitBeamToRAMBeamMapping;
            var annotateStudCountsTransaction = new Transaction(document, "Annotate Beam Stud Counts");
            annotateStudCountsTransaction.Start();
            foreach (var revitBeam in revitBeamToRAMBeamMappingDict.Keys)
            {
                revitBeam.StudCount = revitBeamToRAMBeamMappingDict[revitBeam].StudCount.ToString();
                ElementId beamId = new ElementId(revitBeam.ElementId);
                var revitBeamInstance = document.GetElement(beamId) as FamilyInstance;
                var studCountParameter = revitBeamInstance.LookupParameter("Number of studs");
                Int32.TryParse(revitBeam.StudCount, out int studCount);
                if (studCount==0)
                {
                    studCountParameter.Set("");
                }
                else
                {
                    studCountParameter.Set(revitBeam.StudCount);
                }
                beamIds.Add(revitBeam.ElementId);
            }
            annotateStudCountsTransaction.Commit();
            UpdateVisualizationHistoryWithMappedBeams(beamIds, visualizationHistory, annotationType);

        }

        public static void AnnotateCamberValues(Document document, ModelCompare.Results results, VisualizationHistory visualizationHistory)
        {
            var annotationType = AnnotationType.Camber;
            var beamIds = new List<int>();
            var revitBeamToRAMBeamMappingDict = results.RevitBeamToRAMBeamMapping;
            var annotateCamberValuesTransaction = new Transaction(document, "Annotate Beam Camber Values");
            annotateCamberValuesTransaction.Start();
            foreach (var revitBeam in revitBeamToRAMBeamMappingDict.Keys)
            {
                revitBeam.Camber = revitBeamToRAMBeamMappingDict[revitBeam].Camber.ToString();
                ElementId beamId = new ElementId(revitBeam.ElementId);
                var revitBeamInstance = document.GetElement(beamId) as FamilyInstance;
                var camberParameter = revitBeamInstance.LookupParameter("Camber Size");
                Double.TryParse(revitBeam.Camber, out double camber);
                if(camber == 0)
                {
                    camberParameter.Set("");
                }
                else
                {
                    camberParameter.Set(revitBeam.Camber).ToString();
                }
                beamIds.Add(revitBeam.ElementId);
            }
            annotateCamberValuesTransaction.Commit();
            UpdateVisualizationHistoryWithMappedBeams(beamIds, visualizationHistory, annotationType);

        }

        // TODO:
        public static void ChangeSize(Document document, ModelCompare.Results results)
        {
            var beamsToAnnotate = results.MappedRevitBeams;
            foreach (var beamToAnnotate in beamsToAnnotate)
            {
                ElementId beamId = new ElementId(beamToAnnotate.ElementId);
                var revitBeam = document.GetElement(beamId) as FamilyInstance;
                var revitBeamSize = beamToAnnotate.Size;
                revitBeamSize = revitBeamSize.Replace(" ", "");
                var beamToAnnotateRAMSize = beamToAnnotate.RAMSize;
                if(revitBeamSize != beamToAnnotateRAMSize)
                {
                    //revitBeam.Symbol = new FamilySymbol()
                }
                else
                {
                    continue;
                }


            }
        }





    }

        public class ResultsVisualizer
    {
        private Document _document = null;
        public Dictionary<ColorMapCategories, Color> ColorMap { get; set; }
        public bool BeamReactionsImported { get; set; }
        public bool BeamStudCountsImported { get; set; }
        public bool BeamCamberValuesImported { get; set; }
        public bool BeamSizesImported { get; set; }
        public List<Beam> UserDefinedBeams { get; set; }
        public List<Beam> UnMappedBeams { get; set; }
        public string AnnotationToVisualize { get; set; }

        public ResultsVisualizer(Document document)
        {
            _document = document;

            ColorMap = new Dictionary<ColorMapCategories, Color>();
            ColorMap.Add(ColorMapCategories.Null, new Color(210, 0, 0)); // RED
            ColorMap.Add(ColorMapCategories.UserInput, new Color(0, 0, 230)); // BLUE
            ColorMap.Add(ColorMapCategories.RAMImport, new Color(0, 190, 0)); // GREEN

            ColorMap.Add(ColorMapCategories.SameSizeBeam, new Color(0, 190, 0)); // GREEN
            ColorMap.Add(ColorMapCategories.DifferentSizeBeam, new Color(255, 128, 0)); // ORANGE

        }

        public ResultsVisualizer(Document document, bool beamReactionsImported, bool beamStudCountsImported, bool beamCamberValuesImported, bool beamSizesImported, string annotationToVisualize)
        {
            _document = document;

            ColorMap = new Dictionary<ColorMapCategories, Color>();
            ColorMap.Add(ColorMapCategories.Null, new Color(210, 0, 0)); // RED
            ColorMap.Add(ColorMapCategories.UserInput, new Color(0, 0, 230)); // BLUE
            ColorMap.Add(ColorMapCategories.RAMImport, new Color(0, 190, 0)); // GREEN

            ColorMap.Add(ColorMapCategories.SameSizeBeam, new Color(0, 190, 0)); // GREEN
            ColorMap.Add(ColorMapCategories.DifferentSizeBeam, new Color(255, 128, 0)); // ORANGE

            BeamReactionsImported = beamReactionsImported;
            BeamStudCountsImported = beamStudCountsImported;
            BeamCamberValuesImported = beamCamberValuesImported;
            BeamSizesImported = beamSizesImported;
            AnnotationToVisualize = annotationToVisualize;
        }


        public enum ColorMapCategories
        {
            Null,
            RAMImport,
            UserInput,
            SameSizeBeam,
            DifferentSizeBeam
        }

        public List<Beam> GetUnMappedBeams(List<Beam> mappedRevitBeams, List<Beam> modelBeams)
        {
            var unMappedBeams = new List<Beam>();
            foreach(var modelBeam in modelBeams)
            {
                if(!modelBeam.IsMappedToRAMBeam)
                {
                    unMappedBeams.Add(modelBeam);
                }

            }
            return unMappedBeams;
        }

        public List<Beam> GetUnMappedBeamsForVisualization(VisualizationHistory vh, List<Beam> modelBeams, string annotationToVisualize)
        {
            var unMappedBeams = new List<Beam>();
            var bvses = vh.BeamVisualizationStatuses;
            if (annotationToVisualize == "VisualizeRAMReactions")
            {
                unMappedBeams = GetUnmappedBeamsReactions(bvses, modelBeams);
            }
            else if (annotationToVisualize == "VisualizeRAMSizes")
            {
                return unMappedBeams;
                // TODO:
            }
            else if (annotationToVisualize == "VisualizeRAMStuds")
            {
                unMappedBeams = GetUnmappedBeamsStuds(bvses, modelBeams);
            }
            else if (annotationToVisualize == "VisualizeRAMCamber")
            {
                unMappedBeams = GetUnmappedBeamsCamber(bvses, modelBeams);
            }

            UnMappedBeams = unMappedBeams;
            return unMappedBeams;
        }

        private static List<Beam> GetUnmappedBeamsReactions(List<VisualizationHistory.BeamVisualizationStatus> bvses, List<Beam> modelBeams)
        {
            var unMappedBeams = new List<Beam>();
            foreach (var beam in modelBeams)
            {
                var beamVizStatus = bvses.First(bvs => bvs.BeamId == beam.ElementId);
                if (beamVizStatus.Reactions == VisualizationStatus.Unmapped)
                {
                    unMappedBeams.Add(beam);
                }
            }
            return unMappedBeams;
        }

        private static List<Beam> GetUnmappedBeamsStuds(List<VisualizationHistory.BeamVisualizationStatus> bvses, List<Beam> modelBeams)
        {
            var unMappedBeams = new List<Beam>();
            foreach (var beam in modelBeams)
            {
                var beamVizStatus = bvses.First(bvs => bvs.BeamId == beam.ElementId);
                if (beamVizStatus.Studcount == VisualizationStatus.Unmapped)
                {
                    unMappedBeams.Add(beam);
                }
            }
            return unMappedBeams;
        }

        private static List<Beam> GetUnmappedBeamsCamber(List<VisualizationHistory.BeamVisualizationStatus> bvses, List<Beam> modelBeams)
        {
            var unMappedBeams = new List<Beam>();
            foreach (var beam in modelBeams)
            {
                var beamVizStatus = bvses.First(bvs => bvs.BeamId == beam.ElementId);
                if (beamVizStatus.CamberSize == VisualizationStatus.Unmapped)
                {
                    unMappedBeams.Add(beam);
                }
            }
            return unMappedBeams;
        }


        private bool IsRelevantAnnotationDataImported(string annotationToVisualize)
        {
            if (annotationToVisualize == "VisualizeRAMReactions")
            {
                return BeamReactionsImported;
            }
            if (annotationToVisualize == "VisualizeRAMSizes")
            {
                return BeamSizesImported;
            }
            if (annotationToVisualize == "VisualizeRAMStuds")
            {
                return BeamStudCountsImported;
            }
            if (annotationToVisualize == "VisualizeRAMCamber")
            {
                return BeamCamberValuesImported;
            }
            else throw new Exception("Need to debug: Unrecognized Annotation To Visualize.");
        }

        private void ColorMembersBasedOnData(OverrideGraphicSettings ogs, List<Beam> modelBeams,List<Beam> wrongSizedBeams, List<Beam> rightSizedBeams, string annotationToVisualize, VisualizationHistory visualizationHistory)
        {
            //ChooseAnnotationToVisualizeDataImported(annotationToVisualize, wrongSizedBeams, rightSizedBeams);

            if (annotationToVisualize == "VisualizeRAMSizes")
            {
                // Color Un-mapped Beams in Red.
                var colorNullElem = new Transaction(_document, "Color Unmapped Beams");
                colorNullElem.Start();
                Color nullColor = ColorMap[ColorMapCategories.Null];
                foreach (var beam in UnMappedBeams)
                {
                    ogs.SetProjectionLineColor(nullColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                colorNullElem.Commit();

                // Color Beams in Revit that were mapped with RAM.
                var colorRAMImportElem = new Transaction(_document, "Color RAM Import Beams");
                colorRAMImportElem.Start();
                Color sameSizedBeamColor = ColorMap[ColorMapCategories.SameSizeBeam];
                Color differentSizedBeamColor = ColorMap[ColorMapCategories.DifferentSizeBeam];

                foreach (var beam in rightSizedBeams)
                {
                    ogs.SetProjectionLineColor(sameSizedBeamColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                foreach (var beam in wrongSizedBeams)
                {
                    ogs.SetProjectionLineColor(differentSizedBeamColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                colorRAMImportElem.Commit();
            }
            else
            {
                var colorNullElem = new Transaction(_document, "Color Null Beams");
                colorNullElem.Start();
                Color nullColor = ColorMap[ColorMapCategories.Null];
                Color userInputColor = ColorMap[ColorMapCategories.UserInput];

                foreach (var beam in UnMappedBeams)
                {
                    ogs.SetProjectionLineColor(nullColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                foreach (var beam in UserDefinedBeams)
                {
                    ogs.SetProjectionLineColor(userInputColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                colorNullElem.Commit();

                // Color Beams in Revit that were mapped with RAM.
                // Get Mapped Revit Beams.
                var mappedRevitBeams = GetMappedRevitBeams(visualizationHistory, modelBeams, annotationToVisualize);
                var colorRAMImportElem = new Transaction(_document, "Color RAM Import Beams");
                colorRAMImportElem.Start();
                Color ramImportColor = ColorMap[ColorMapCategories.RAMImport];
                foreach (var beam in mappedRevitBeams)
                {
                    ogs.SetProjectionLineColor(ramImportColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                colorRAMImportElem.Commit();
            }
        }

        private List<Beam> GetMappedRevitBeams(VisualizationHistory visualizationHistory, List<Beam> modelBeams, string annotationToVisualize)
        {
            var mappedBeams = new List<Beam>();
            var bvses = visualizationHistory.BeamVisualizationStatuses;
            if (annotationToVisualize == "VisualizeRAMReactions")
            {
                foreach (var beam in modelBeams)
                {
                    int beamId = beam.ElementId;
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                    if (beamVizStatus.Reactions == VisualizationStatus.Mapped)
                    {
                        mappedBeams.Add(beam);
                    }
                }
            }
            if (annotationToVisualize == "VisualizeRAMStuds")
            {
                foreach (var beam in modelBeams)
                {
                    int beamId = beam.ElementId;
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                    if (beamVizStatus.Studcount == VisualizationStatus.Mapped)
                    {
                        mappedBeams.Add(beam);
                    }
                }
            }
            if (annotationToVisualize == "VisualizeRAMCamber")
            {
                foreach (var beam in modelBeams)
                {
                    int beamId = beam.ElementId;
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                    if (beamVizStatus.CamberSize == VisualizationStatus.Mapped)
                    {
                        mappedBeams.Add(beam);
                    }
                }
            }
            if (annotationToVisualize == "VisualizeRAMSizes")
            {
                foreach (var beam in modelBeams)
                {
                    int beamId = beam.ElementId;
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                    if (beamVizStatus.BeamSize == VisualizationStatus.Mapped)
                    {
                        mappedBeams.Add(beam);
                    }
                }
            }
            return mappedBeams;
        }

        // REACTIONS.
        public List<Beam> UpdateVisulationHistoryWithUnMappedMembers(string annotationToVisualize, List<Beam> unMappedBeams, VisualizationHistory visualizationHistory)
        {
            var userDefinedBeams = new List<Beam>();
            foreach(var beam in unMappedBeams)
            {
                int beamId = beam.ElementId;
                if (annotationToVisualize == "VisualizeRAMReactions")
                {
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                    if(beamVizStatus.Reactions == VisualizationStatus.UserDefined)
                    {
                        userDefinedBeams.Add(beam);
                        continue;
                    }
                    else
                    {
                        beamVizStatus.Reactions = VisualizationStatus.Unmapped;
                    }
                }
                if (annotationToVisualize == "VisualizeRAMSizes")
                {
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);

                    if (beamVizStatus.BeamSize == VisualizationStatus.UserDefined)
                    {
                        userDefinedBeams.Add(beam);
                        continue;
                    }
                    else
                    {
                        beamVizStatus.BeamSize = VisualizationStatus.Unmapped;
                    }
                }
                if (annotationToVisualize == "VisualizeRAMStuds")
                {
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                    if (beamVizStatus.Studcount == VisualizationStatus.UserDefined)
                    {
                        userDefinedBeams.Add(beam);
                        continue;
                    }
                    else
                    {
                        beamVizStatus.Studcount = VisualizationStatus.Unmapped;
                    }
                }
                if (annotationToVisualize == "VisualizeRAMCamber")
                {
                    var beamVizStatus = visualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == beamId);
                    if (beamVizStatus.CamberSize == VisualizationStatus.UserDefined)
                    {
                        userDefinedBeams.Add(beam);
                        continue;
                    }
                    else
                    {
                        beamVizStatus.CamberSize = VisualizationStatus.Unmapped;
                    }
                }
            }
            UserDefinedBeams = userDefinedBeams;
            unMappedBeams = unMappedBeams.Except(userDefinedBeams).ToList();
            UnMappedBeams = unMappedBeams;
            return unMappedBeams;
        }

        public List<Beam> GetUserDefinedBeamsForVisualization(VisualizationHistory vh, List<Beam> modelBeams, string annotationToVisualize)
        {
            var userDefinedBeams = new List<Beam>();
            var bvses = vh.BeamVisualizationStatuses;
            if (annotationToVisualize == "VisualizeRAMReactions")
            {
                userDefinedBeams = GetUserDefinedBeamsReactions(modelBeams, bvses);
            }
            else if (annotationToVisualize == "VisualizeRAMSizes")
            {
                return userDefinedBeams;
                // TODO:
            }
            else if (annotationToVisualize == "VisualizeRAMStuds")
            {
                userDefinedBeams = GetUserDefinedBeamsStuds(modelBeams, bvses);
            }
            else if (annotationToVisualize == "VisualizeRAMCamber")
            {
                userDefinedBeams = GetUserDefinedBeamsCamber(modelBeams, bvses);
            }
            UserDefinedBeams = userDefinedBeams;
            return userDefinedBeams;
        }

        private List<Beam> GetUserDefinedBeamsReactions(List<Beam> modelBeams, List<VisualizationHistory.BeamVisualizationStatus> bvses)
        {
            var userDefinedBeams = new List<Beam>();
            foreach (var beam in modelBeams)
            {
                var beamVizStatus = bvses.First(bvs => bvs.BeamId == beam.ElementId);
                if (beamVizStatus.Reactions == VisualizationStatus.UserDefined)
                {
                    userDefinedBeams.Add(beam);
                }
            }
            return userDefinedBeams;
        }

        private List<Beam> GetUserDefinedBeamsCamber(List<Beam> modelBeams, List<VisualizationHistory.BeamVisualizationStatus> bvses)
        {
            var userDefinedBeams = new List<Beam>();
            foreach (var beam in modelBeams)
            {
                var beamVizStatus = bvses.First(bvs => bvs.BeamId == beam.ElementId);
                if (beamVizStatus.CamberSize == VisualizationStatus.UserDefined)
                {
                    userDefinedBeams.Add(beam);
                }
            }
            return userDefinedBeams;
        }

        private List<Beam> GetUserDefinedBeamsStuds(List<Beam> modelBeams, List<VisualizationHistory.BeamVisualizationStatus> bvses)
        {
            var userDefinedBeams = new List<Beam>();
            foreach (var beam in modelBeams)
            {
                var beamVizStatus = bvses.First(bvs => bvs.BeamId == beam.ElementId);
                if (beamVizStatus.Studcount == VisualizationStatus.UserDefined)
                {
                    userDefinedBeams.Add(beam);
                }
            }
            return userDefinedBeams;
        }

        public void ColorMembers(AnalyticalModel model, string annotationToVisualize, List<Beam> mappedRevitBeams, List<Beam> modelBeams, VisualizationHistory visualizationHistory)
        {
            var unMappedBeams = UnMappedBeams;
            //if (IsRelevantAnnotationDataImported(annotationToVisualize))
            //{
            //    unMappedBeams = GetUnMappedBeams(mappedRevitBeams, modelBeams);
            //    mappedBeams = mappedRevitBeams;
            //}
            //else
            //{
            //    unMappedBeams = modelBeams;
            //}

            var nullBeams = new List<Beam>();
            var userInputBeams = new List<Beam>();
            var wrongSizedBeams = new List<Beam>();
            var rightSizedBeams = new List<Beam>();
            var projectPatterns = new FilteredElementCollector(_document);
            projectPatterns.OfClass(typeof(FillPatternElement));
            var patternIds = (IList<ElementId>)projectPatterns.ToElementIds();
            var ogs = new OverrideGraphicSettings();
            ogs.SetProjectionLineWeight(10);
            
            ColorMembersBasedOnData(ogs, modelBeams, wrongSizedBeams, rightSizedBeams, annotationToVisualize, visualizationHistory);
        }

        private List<ElementId> GetBeamsToTrack(Autodesk.Revit.DB.View activeView, List<Beam> modelBeams)
        {
            var beamsToTrack = new List<ElementId>();
            var levelActiveView = activeView.GenLevel;
            int levelIdOfActiveView = Int32.Parse(levelActiveView.Id.ToString());
            foreach (var beam in modelBeams)
            {
                if(beam.ElementLevelId == levelIdOfActiveView)
                {
                    beamsToTrack.Add(new ElementId(beam.ElementId));
                }
                else
                {
                    continue;
                }
            }
            return beamsToTrack;
        }

        public void VisualizationTrigger(List<Beam> modelBeams, ParameterUpdater updater, string annotationToVisualize, Autodesk.Revit.DB.View activeView)
        {
            Parameter parameterToTrack1 = null;
            Parameter parameterToTrack2 = null;

            var beamsToTrack = GetBeamsToTrack(activeView, modelBeams);
            var revitBeamForParam = _document.GetElement(new ElementId(modelBeams[0].ElementId)) as FamilyInstance;
            if (annotationToVisualize == "VisualizeRAMReactions")
            {
                parameterToTrack1 = revitBeamForParam.LookupParameter("Start Reaction - Total");
                parameterToTrack2 = revitBeamForParam.LookupParameter("End Reaction - Total"); ;
            }
            if (annotationToVisualize == "VisualizeRAMSizes")
            {
                //parameterToTrack = revitBeamForParam.LookupParameter("Number of studs"); ;
            }
            if (annotationToVisualize == "VisualizeRAMStuds")
            {
                parameterToTrack1 = revitBeamForParam.LookupParameter("Number of studs"); ;
            }
            if (annotationToVisualize == "VisualizeRAMCamber")
            {
                parameterToTrack1 = revitBeamForParam.LookupParameter("Camber Size"); ;
            }

            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), _document,
                beamsToTrack, Element.GetChangeTypeParameter(parameterToTrack1));

            if (annotationToVisualize == "VisualizeRAMReactions")
            {
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), _document,
                    beamsToTrack, Element.GetChangeTypeParameter(parameterToTrack2));
            }


        }

        //public void ChooseAnnotationToVisualizeDataImported(string annotationToVisualize, List<Beam> wrongSizedBeams, List<Beam> rightSizedBeams, List<Beam> mappedBeams, VisualizationHistory visualizationHistory)
        //{
        //    if (annotationToVisualize == "VisualizeRAMReactions")
        //    {
        //        CategorizeReactionValues(visualizationHistory);
        //    }
        //    if (annotationToVisualize == "VisualizeRAMSizes")
        //    {
        //        CategorizeSizeValues(wrongSizedBeams, rightSizedBeams, mappedBeams);
        //    }
        //    if (annotationToVisualize == "VisualizeRAMStuds")
        //    {
        //        CategorizeStudCountValues(visualizationHistory);

        //    }
        //    if (annotationToVisualize == "VisualizeRAMCamber")
        //    {
        //        CategorizeCamberValues(visualizationHistory);
        //    }
        //}

        //public void ChooseAnnotationToVisualizeDataNotImported(List<Beam> nullBeams, List<Beam> userInputBeams, string annotationToVisualize, List<Beam> modelBeams)
        //{
        //    if (annotationToVisualize == "VisualizeRAMReactions")
        //    {
        //        CategorizeReactionValuesNoData(modelBeams, nullBeams, userInputBeams);
        //    }
        //    if (annotationToVisualize == "VisualizeRAMSizes")
        //    {
        //        return;
        //    }
        //    if (annotationToVisualize == "VisualizeRAMStuds")
        //    {
        //        CategorizeStudCountValuesNoData(nullBeams, userInputBeams);

        //    }
        //    if (annotationToVisualize == "VisualizeRAMCamber")
        //    {
        //        CategorizeCamberValuesNoData(nullBeams, userInputBeams);
        //    }
        //}

            // REACTIONS.
        //    public void CategorizeReactionValues(VisualizationHistory visualizationHistory)
        //{
        //    foreach (var beam in unMappedBeams)
        //    {
        //        ElementId beamId = new ElementId(beam.ElementId);
        //        var revitBeam = _document.GetElement(beamId) as FamilyInstance;
        //        var startReactionTotalParameter = revitBeam.LookupParameter("Start Reaction - Total");
        //        var endReactionTotalParameter = revitBeam.LookupParameter("End Reaction - Total");

        //        bool startRxnParameterHasValue = startReactionTotalParameter.HasValue;
        //        bool endRxnParameterHasValue = endReactionTotalParameter.HasValue;

        //        if (!startRxnParameterHasValue || !endRxnParameterHasValue)
        //            {
        //                nullBeams.Add(beam);
        //            }
        //            else
        //            {
        //                userInputBeams.Add(beam);
        //            }
        //    }
        //}

        //public void CategorizeReactionValuesNoData(List<Beam>  modelBeams, List<Beam> nullBeams, List<Beam> userInputBeams)
        //{
        //    foreach (var beam in unMappedBeams)
        //    {
        //        ElementId beamId = new ElementId(beam.ElementId);
        //        var revitBeam = _document.GetElement(beamId) as FamilyInstance;
        //        var startReactionTotalParameter = revitBeam.LookupParameter("Start Reaction - Total");
        //        var endReactionTotalParameter = revitBeam.LookupParameter("End Reaction - Total");

        //        bool startRxnParameterHasValue = startReactionTotalParameter.HasValue;
        //        bool endRxnParameterHasValue = endReactionTotalParameter.HasValue;

        //        if (!startRxnParameterHasValue || !endRxnParameterHasValue)
        //        {
        //            nullBeams.Add(beam);
        //        }
        //        else
        //        {
        //            userInputBeams.Add(beam);
        //        }
        //    }
        //}


        // STUDS.
        public void CategorizeStudCountValues(List<Beam> unMappedBeams, List<Beam> nullBeams, List<Beam> userInputBeams)
        {
            foreach (var beam in unMappedBeams)
            {
                ElementId beamId = new ElementId(beam.ElementId);
                var revitBeam = _document.GetElement(beamId) as FamilyInstance;
                var studCount = revitBeam.LookupParameter("Number of studs");

                if (!studCount.HasValue)
                {
                    nullBeams.Add(beam);
                }
                else
                {
                    userInputBeams.Add(beam);
                }
            }
        }

        // CAMBER.
        public void CategorizeCamberValues(List<Beam> unMappedBeams, List<Beam> nullBeams, List<Beam> userInputBeams)
        {
            foreach (var beam in unMappedBeams)
            {
                ElementId beamId = new ElementId(beam.ElementId);
                var revitBeam = _document.GetElement(beamId) as FamilyInstance;
                var camberSize = revitBeam.LookupParameter("Camber Size");

                if (!camberSize.HasValue)
                {
                    nullBeams.Add(beam);
                }
                else
                {
                    userInputBeams.Add(beam);
                }
            }
        }

        // SIZES.
        public void CategorizeSizeValues(List<Beam> wrongSizedBeams, List<Beam> rightSizedBeams, List<Beam> mappedBeams)
        {
            foreach (var revitBeam in mappedBeams)
            {
                //ElementId beamId = new ElementId(beam.ElementId);
                //var revitBeam = _document.GetElement(beamId) as FamilyInstance;
                var revitBeamSize = revitBeam.Size;
                var ramBeamSize = revitBeam.RAMSize;

                if (revitBeamSize == ramBeamSize)
                {
                    rightSizedBeams.Add(revitBeam);
                }
                else
                {
                    rightSizedBeams.Add(revitBeam);
                }
            }


        }
        public void ResetVisualsInActiveView(List<Beam> modelBeams)
        {
            var ogs = new OverrideGraphicSettings();

            foreach (var elem in modelBeams)
            {
                var reset = new Transaction(_document, "Reset One Element");
                reset.Start();
                _document.ActiveView.SetElementOverrides(new ElementId(elem.ElementId), ogs);
                //_document.ActiveView.DisplayStyle = DisplayStyle.Shading;
                reset.Commit();
            }
        }

        // CLEAR DATA.
        public void ClearSelectedAnnotations()
        {

        }
        //public string GetAnnotationToVisualize()
        //{
        //    return AnnotationToVisualize;
        //}

        public class ParameterUpdater : IUpdater
        {
            public UpdaterId _uid;
            public List<int> ModifiedElementIds { get; set; }
            private ExternalEvent beamInstanceParameterHasBeenModified;
            public VisualizationHistory VisualizationHistory { get; set; }
            private UpdaterData UpdaterData { get; set; }
            public string ProjectId { get; set; }

            public ParameterUpdater(Guid guid, VisualizationHistory visualizationHistory, string projectId)
            {
                _uid = new UpdaterId(new AddInId(
                    new Guid("46e366ec-491c-4fad-906d-51c00f43c9c8")), // addin id
                    guid); // updater id

                VisualizationHistory = visualizationHistory;
                ProjectId = projectId;
                var beamInstanceParamterModifedHandler = new BeamInstanceParamterModifedHandler();
                beamInstanceParamterModifedHandler.ParamUpdater = this;
                beamInstanceParameterHasBeenModified = ExternalEvent.Create(beamInstanceParamterModifedHandler);
                ModifiedElementIds = new List<int>();
            }

            public void GetModifiedElementIds(List<ElementId> modifiedElementIds)
            {
                foreach (var elementId in modifiedElementIds)
                {
                    
                    int elementIdInt = elementId.IntegerValue;
                    ModifiedElementIds.Add(elementIdInt);
                }
                
            }

            public void UpdateVisualizationHistoryWithNewUserDefinedParam()
            {
                var bvses = VisualizationHistory.BeamVisualizationStatuses;
                foreach (int id in ModifiedElementIds)
                {
                    var beamVizStatus = VisualizationHistory.BeamVisualizationStatuses.First(bvs => bvs.BeamId == id);
                    beamVizStatus.Reactions = VisualizationStatus.UserDefined;
                }
                SaveVisualizationHistoryToDisk();
            }

            public void Execute(UpdaterData data)
            {
                UpdaterData = data;
                var modifiedElementIds = data.GetModifiedElementIds().ToList();
                GetModifiedElementIds(modifiedElementIds);

                if (beamInstanceParameterHasBeenModified != null)
                {
                    beamInstanceParameterHasBeenModified.Raise();
                }
                else MessageBox.Show("BeamInstanceParameterHasBeenModified event handler is null");

                MessageBox.Show("DMU Working");
            }

            public string GetAdditionalInformation()
            {
                return "N/A";
            }

            public ChangePriority GetChangePriority()
            {
                return ChangePriority.FreeStandingComponents;
            }

            public UpdaterId GetUpdaterId()
            {
                return _uid;
            }

            public string GetUpdaterName()
            {
                return "ParameterUpdater";
            }

            public void CleanUpdaterRegistry()
            {
                UpdaterRegistry.UnregisterUpdater(_uid);
            }

            public void RemoveAllTriggers()
            {
                UpdaterRegistry.RemoveAllTriggers(_uid);
            }

            public void StopTracking()
            {
                RemoveAllTriggers();
                CleanUpdaterRegistry();
                this._uid.Dispose();
            }

            public void SaveVisualizationHistoryToDisk()
            {
                EnsureVisualizationHistoryDirectoryExists();

                string fullPath = GetVisualizationHistoryFile(ProjectId);

                var histJson = JsonConvert.SerializeObject(VisualizationHistory, Formatting.Indented, new StringEnumConverter());

                System.IO.File.WriteAllText(fullPath, histJson);
            }

            private static string GetVisualizationHistoryFile(string projectId)
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = System.IO.Path.Combine(folder, @"RevitRxnImporter\visualization_history");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return System.IO.Path.Combine(dir, string.Format("Project-{0}_visualization_history.txt", projectId));
            }

            private void EnsureVisualizationHistoryDirectoryExists()
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = System.IO.Path.Combine(folder, "RevitRxnImporter");

                if (Directory.Exists(dir))
                    return;

                Directory.CreateDirectory(dir);
            }

        }



    }
}
