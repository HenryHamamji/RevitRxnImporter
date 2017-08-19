using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Forms;

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
                double locX = ((revitBeam.StartPoint[0] / 12.0) + (revitBeam.EndPoint[0] / 12.0)) / 2.0;
                double locY = ((revitBeam.StartPoint[1] / 12.0) + (revitBeam.EndPoint[1] / 12.0)) / 2.0;
                double locZ = ((revitBeam.StartPoint[2] / 12.0) + (revitBeam.EndPoint[2] / 12.0)) / 2.0;
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

        private void AddStartReactionTag(Beam revitBeam, Document document, FamilyInstance revitBeamInstance, Autodesk.Revit.Creation.Document createDoc)
        {
            double locX = ((revitBeam.StartPoint[0] / 12.0) + (revitBeam.EndPoint[0] / 12.0)) / 2.0;
            double locY = ((revitBeam.StartPoint[1] / 12.0) + (revitBeam.EndPoint[1] / 12.0)) / 2.0;
            double locZ = ((revitBeam.StartPoint[2] / 12.0) + (revitBeam.EndPoint[2] / 12.0)) / 2.0;
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
            double locX = ((revitBeam.StartPoint[0] / 12.0) + (revitBeam.EndPoint[0] / 12.0)) / 2.0;
            double locY = ((revitBeam.StartPoint[1] / 12.0) + (revitBeam.EndPoint[1] / 12.0)) / 2.0;
            double locZ = ((revitBeam.StartPoint[2] / 12.0) + (revitBeam.EndPoint[2] / 12.0)) / 2.0;
            var endLocation = new XYZ(locX, locY, locZ);
            IndependentTag endTag = createDoc.NewTag(document.ActiveView, revitBeamInstance, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, endLocation);
            endTag.ChangeTypeId(new ElementId(EndReactionTagId));

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


        public static void FillInBeamParameterValues(Document document, ModelCompare.Results results, AnnotationType annotationType)
        {
            if(annotationType == AnnotationType.Reaction)
            {
                AssignReactions(document, results);
            }
            else if (annotationType == AnnotationType.StudCount)
            {
                AnnotateStudCounts(document, results);
            }
            else if (annotationType == AnnotationType.Camber)
            {
                AnnotateCamberValues(document, results);
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

        public static void AssignReactions(Document document, ModelCompare.Results results)
        {
            var revitBeamToRAMBeamMappingDict = results.RevitBeamToRAMBeamMapping;
            var annotateReactionsTransaction = new Transaction(document, "Annotate Beam Reactions");
            annotateReactionsTransaction.Start();
            foreach (var revitBeam in revitBeamToRAMBeamMappingDict.Keys)
            {
                revitBeam.StartReactionTotal = revitBeamToRAMBeamMappingDict[revitBeam].StartTotalReactionPositive.ToString();
                revitBeam.EndReactionTotal = revitBeamToRAMBeamMappingDict[revitBeam].EndTotalReactionPositive.ToString();
                ElementId beamId = new ElementId(revitBeam.ElementId);
                var revitBeamInstance = document.GetElement(beamId) as FamilyInstance;
                var startReactionTotalParameter = revitBeamInstance.LookupParameter("Start Reaction - Total");
                var endReactionTotalParameter = revitBeamInstance.LookupParameter("End Reaction - Total");
                startReactionTotalParameter.SetValueString(revitBeam.StartReactionTotal);
                endReactionTotalParameter.SetValueString(revitBeam.EndReactionTotal);
            }
            annotateReactionsTransaction.Commit();

        }

        public static void AnnotateStudCounts(Document document, ModelCompare.Results results)
        {
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
                    studCountParameter.Set(revitBeam.StudCount);//.ToString();
                }
            }
            annotateStudCountsTransaction.Commit();

        }

        public static void AnnotateCamberValues(Document document, ModelCompare.Results results)
        {
            var revitBeamToRAMBeamMappingDict = results.RevitBeamToRAMBeamMapping;
            var annotateCamberValuesTransaction = new Transaction(document, "Annotate Beam Camber Values");
            annotateCamberValuesTransaction.Start();
            foreach (var revitBeam in revitBeamToRAMBeamMappingDict.Keys)
            {
                revitBeam.Camber = revitBeamToRAMBeamMappingDict[revitBeam].Camber.ToString();
                ElementId beamId = new ElementId(revitBeam.ElementId);
                var revitBeamInstance = document.GetElement(beamId) as FamilyInstance;
                var camberParameter = revitBeamInstance.LookupParameter("Camber Size");
                camberParameter.Set(revitBeam.Camber).ToString();
            }
            annotateCamberValuesTransaction.Commit();

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

        public ResultsVisualizer(Document document, bool beamReactionsImported, bool beamStudCountsImported, bool beamCamberValuesImported, bool beamSizesImported)
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

        private void ColorMembersBasedOnData(OverrideGraphicSettings ogs, List<Beam> mappedRevitBeams, List<Beam> modelBeams, List<Beam> unMappedBeams, List<Beam> nullBeams, List<Beam> userInputBeams, List<Beam> wrongSizedBeams, List<Beam> rightSizedBeams, string annotationToVisualize)
        {
            ChooseAnnotationToVisualizeDataImported(unMappedBeams, nullBeams, userInputBeams, annotationToVisualize, wrongSizedBeams, rightSizedBeams, mappedRevitBeams);

            if (annotationToVisualize == "VisualizeRAMSizes")
            {
                // Color Un-mapped Beams in Red.
                var colorNullElem = new Transaction(_document, "Color Unmapped Beams");
                colorNullElem.Start();
                Color nullColor = ColorMap[ColorMapCategories.Null];
                foreach (var beam in unMappedBeams)
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

                foreach (var beam in nullBeams)
                {
                    ogs.SetProjectionLineColor(nullColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                foreach (var beam in userInputBeams)
                {
                    ogs.SetProjectionLineColor(userInputColor);
                    _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
                }
                colorNullElem.Commit();

                // Color Beams in Revit that were mapped with RAM.
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

        //private void ColorMembersNoDataImported(OverrideGraphicSettings ogs, List<Beam> modelBeams, List<Beam> nullBeams, List<Beam> userInputBeams, string annotationToVisualize)
        //{
        //    ChooseAnnotationToVisualizeDataNotImported(unMappedBeams, nullBeams, userInputBeams, annotationToVisualize, wrongSizedBeams, rightSizedBeams, mappedRevitBeams);

        //    if (annotationToVisualize == "VisualizeRAMSizes")
        //    {
        //        // Color Un-mapped Beams in Red.
        //        var colorNullElem = new Transaction(_document, "Color Unmapped Beams");
        //        colorNullElem.Start();
        //        Color nullColor = ColorMap[ColorMapCategories.Null];
        //        foreach (var beam in unMappedBeams)
        //        {
        //            ogs.SetProjectionLineColor(nullColor);
        //            _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
        //        }
        //        colorNullElem.Commit();

        //        // Color Beams in Revit that were mapped with RAM.
        //        var colorRAMImportElem = new Transaction(_document, "Color RAM Import Beams");
        //        colorRAMImportElem.Start();
        //        Color sameSizedBeamColor = ColorMap[ColorMapCategories.SameSizeBeam];
        //        Color differentSizedBeamColor = ColorMap[ColorMapCategories.DifferentSizeBeam];

        //        foreach (var beam in rightSizedBeams)
        //        {
        //            ogs.SetProjectionLineColor(sameSizedBeamColor);
        //            _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
        //        }
        //        foreach (var beam in wrongSizedBeams)
        //        {
        //            ogs.SetProjectionLineColor(differentSizedBeamColor);
        //            _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
        //        }
        //        colorRAMImportElem.Commit();
        //    }
        //    else
        //    {
        //        var colorNullElem = new Transaction(_document, "Color Null Beams");
        //        colorNullElem.Start();
        //        Color nullColor = ColorMap[ColorMapCategories.Null];
        //        Color userInputColor = ColorMap[ColorMapCategories.UserInput];

        //        foreach (var beam in nullBeams)
        //        {
        //            ogs.SetProjectionLineColor(nullColor);
        //            _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
        //        }
        //        foreach (var beam in userInputBeams)
        //        {
        //            ogs.SetProjectionLineColor(userInputColor);
        //            _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
        //        }
        //        colorNullElem.Commit();

        //        // Color Beams in Revit that were mapped with RAM.
        //        var colorRAMImportElem = new Transaction(_document, "Color RAM Import Beams");
        //        colorRAMImportElem.Start();
        //        Color ramImportColor = ColorMap[ColorMapCategories.RAMImport];
        //        foreach (var beam in mappedRevitBeams)
        //        {
        //            ogs.SetProjectionLineColor(ramImportColor);
        //            _document.ActiveView.SetElementOverrides(new ElementId(beam.ElementId), ogs);
        //        }
        //        colorRAMImportElem.Commit();
        //    }
        //}

        // REACTIONS.
        public void ColorMembers(AnalyticalModel model, string annotationToVisualize, List<Beam> mappedRevitBeams, List<Beam> modelBeams)
        {
            var unMappedBeams = new List<Beam>();
            var mappedBeams = new List<Beam>();

            if (IsRelevantAnnotationDataImported(annotationToVisualize))
            {
                unMappedBeams = GetUnMappedBeams(mappedRevitBeams, modelBeams);
                mappedBeams = mappedRevitBeams;
            }
            else
            {
                unMappedBeams = modelBeams;
            }

            var nullBeams = new List<Beam>();
            var userInputBeams = new List<Beam>();
            var wrongSizedBeams = new List<Beam>();
            var rightSizedBeams = new List<Beam>();
            var projectPatterns = new FilteredElementCollector(_document);
            projectPatterns.OfClass(typeof(FillPatternElement));
            var patternIds = (IList<ElementId>)projectPatterns.ToElementIds();
            var ogs = new OverrideGraphicSettings();
            ogs.SetProjectionLineWeight(10);

            ColorMembersBasedOnData(ogs, mappedBeams, modelBeams, unMappedBeams, nullBeams, userInputBeams, wrongSizedBeams, rightSizedBeams, annotationToVisualize);

        }

        public void ChooseAnnotationToVisualizeDataImported(List<Beam> unMappedBeams, List<Beam> nullBeams, List<Beam> userInputBeams, string annotationToVisualize, List<Beam> wrongSizedBeams, List<Beam> rightSizedBeams, List<Beam> mappedBeams)
        {
            if (annotationToVisualize == "VisualizeRAMReactions")
            {
                CategorizeReactionValues(unMappedBeams, nullBeams, userInputBeams);
            }
            if (annotationToVisualize == "VisualizeRAMSizes")
            {
                CategorizeSizeValues(wrongSizedBeams, rightSizedBeams, mappedBeams);
            }
            if (annotationToVisualize == "VisualizeRAMStuds")
            {
                CategorizeStudCountValues(unMappedBeams, nullBeams, userInputBeams);

            }
            if (annotationToVisualize == "VisualizeRAMCamber")
            {
                CategorizeCamberValues(unMappedBeams, nullBeams, userInputBeams);
            }
        }

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
            public void CategorizeReactionValues(List<Beam> unMappedBeams, List<Beam> nullBeams, List<Beam> userInputBeams)
        {
            foreach (var beam in unMappedBeams)
            {
                ElementId beamId = new ElementId(beam.ElementId);
                var revitBeam = _document.GetElement(beamId) as FamilyInstance;
                var startReactionTotalParameter = revitBeam.LookupParameter("Start Reaction - Total");
                var endReactionTotalParameter = revitBeam.LookupParameter("End Reaction - Total");

                bool startRxnParameterHasValue = startReactionTotalParameter.HasValue;
                bool endRxnParameterHasValue = endReactionTotalParameter.HasValue;

                if (!startRxnParameterHasValue || !endRxnParameterHasValue)
                    {
                        nullBeams.Add(beam);
                    }
                    else
                    {
                        userInputBeams.Add(beam);
                    }
            }
        }

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


        }
}
