using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitReactionImporter
{
    public class ModelCompare
    {
        //private List<RAMModel.Story> levelsRAM;
        //private List<LevelFloor> levelsRevit;
        //public List<RAMModel.Story> LevelsRAM {get { return levelsRAM;  } }
        //public List<LevelFloor> LevelsRevit { get { return levelsRevit; } }
        public List<RAMModel.Story> StoriesRAM { get; set; }
        public List<RAMModel.RAMBeam> Beams { get; set; }
        //public List<LevelFloor> LevelsRevit { get; set; }
        public LevelInfo RevitLevelInfo { get; set; }
        public Dictionary<int, string> LevelNameMapping { get; set; }
        public Dictionary<int, string> LevelElevationMapping { get; set; }
        public Dictionary<int, string> LevelOrderMapping { get; set; }
        public Dictionary<int, string> LevelSpacingMapping { get; set; }
        public Dictionary<int, string> LevelMapping { get; set; }
        public Dictionary<string, double> LevelsRevitSpacings { get; set; }
        public int MappedBeamsCount { get; set; }

        public class Results
        {
            public Dictionary<int, string> LevelMapping { get; set; }
            public int TotalMappedBeamCount { get; set; }
            public Dictionary<string, string> RAMMappingResultsByFloor { get; set; }
            public Dictionary<string, string> RevitMappingResultsByFloor { get; set; }
            public Dictionary<string, string> RevitLevelToRAMFloorLayoutTypeMapping { get; set; }
            public Dictionary<string, double> RevitBeamTolerancesForMappingSuccess { get; set; }

            public Results(Dictionary<int, string> levelMapping)
            {
                LevelMapping = levelMapping;
                //RAMMappingResultsByFloor = ramMappingResultsByFloor;
                //RevitMappingResultsByFloor = revitMappingResultsByFloor;
                //, Dictionary<string, string> ramMappingResultsByFloor, Dictionary<string, string> revitMappingResultsByFloor
            }
        }

        public ModelCompare(RAMModel ramModel, AnalyticalModel revitModel)
        {
            StoriesRAM = ramModel.Stories;
            Beams = ramModel.RamBeams;
            RevitLevelInfo = revitModel.LevelInfo;
            //LevelsRevit = revitModel.LevelInfo.Levels;
            LevelNameMapping = new Dictionary<int, string>();
            LevelElevationMapping = new Dictionary<int, string>();
            LevelOrderMapping = new Dictionary<int, string>();
            LevelSpacingMapping = new Dictionary<int, string>();
            LevelMapping = new Dictionary<int, string>();
            LevelsRevitSpacings = new Dictionary<string, double>();

            foreach (var revitLevel in revitModel.LevelInfo.Levels)
            {
                LevelNameMapping[revitLevel.ElementId] = "None";
                LevelElevationMapping[revitLevel.ElementId] = "None";
                LevelOrderMapping[revitLevel.ElementId] = "None";
                LevelMapping[revitLevel.ElementId] = "None";
            }
        }

        public static Results CompareModels(RAMModel ramModel, AnalyticalModel revitModel)
        {
            var modelCompare = new ModelCompare(ramModel, revitModel);
            // Grid Mapping.
            foreach (var revitGrid in revitModel.GridData.Grids)
            {
                ClassifyGridNameType(revitGrid);

            }
            ClassifyGridDirectionalities(revitModel.GridData.Grids);

            // Level Mapping.
            bool levelFilteringNotRequired = FilteringLevelStoryDataNotRequired(ramModel, revitModel);
            if(!levelFilteringNotRequired)
            {
                FilterLevelStoryData(ramModel, revitModel);
            }
            if(ramModel.Stories.Count == revitModel.LevelInfo.LevelsRevitSpacings.Count)
            {
                PerformLevelSpacingMappingEqualStoryCount(ramModel, revitModel, modelCompare.LevelSpacingMapping);
            }
            PerformLevelMapping(ramModel.Stories, revitModel.LevelInfo, modelCompare.LevelNameMapping, modelCompare.LevelElevationMapping, modelCompare.LevelOrderMapping, modelCompare.LevelMapping, modelCompare.LevelSpacingMapping, revitModel.LevelInfo.BaseReferenceElevation);

            // Beam Mapping.
            var results = new Results(modelCompare.LevelMapping);
            // TODO: tolerances calc function.
            //var tolerances = DetermineTolerances(revitModel.GridData, ramModel.HorizontalGrids, ramModel.VerticalGrids);
            double tolerance = 0.0;
            PerformBeamMapping(ramModel, revitModel, modelCompare.LevelMapping, results, tolerance);

            return results;

        }

        // LEVEL MAPPING.


        public static bool FilteringLevelStoryDataNotRequired(RAMModel ramModel, AnalyticalModel revitModel)
        {
            var levelInfo = revitModel.LevelInfo;
            // If Revit Level & RAM Story counts are matching and all spacings are almost equal then return true (do mapping).
            bool levelStoryCountsMatch = IsLevelStoryCountMatching(revitModel.LevelInfo.LevelCount, ramModel.Stories.Count);
            bool allSpacingsAreEqual = false;
            var revitSpacingsMappingStatus = new Dictionary<string, bool>();
            if (levelStoryCountsMatch)
            {
                for (int i = 0; i < levelInfo.LevelsRevitSpacings.Count; i++)
                {
                    int revitBaseLevelNumber = levelInfo.Levels[i].LevelNumber;
                    int revitTopLevelNumber = levelInfo.Levels[i + 1].LevelNumber;
                    string revitLevelSpacingName = revitBaseLevelNumber.ToString() + '-' + revitTopLevelNumber.ToString();
                    double revitLevelSpacing = levelInfo.LevelsRevitSpacings[revitLevelSpacingName];
                    double ramLevelSpacing = ramModel.Stories[i].Height;
                    if (CompareLevelSpacings(ramLevelSpacing, revitLevelSpacing))
                    {
                        ramModel.Stories[i].MapRevitLevelToThis = true;
                        //revitModel.LevelInfo.Levels[i].MapRAMLayoutTypeToThis = true;
                        revitSpacingsMappingStatus[revitLevelSpacingName] = true;

                    }
                    else
                    {
                        ramModel.Stories[i].MapRevitLevelToThis = false;
                        //revitModel.LevelInfo.Levels[i].MapRAMLayoutTypeToThis = false;
                        revitSpacingsMappingStatus[revitLevelSpacingName] = true;


                    }
                }
                if (revitSpacingsMappingStatus.ContainsValue(false))
                {
                    allSpacingsAreEqual = false;
                }
                else
                {
                    allSpacingsAreEqual = true;
                }
            }
            else
            {
                return false; // Revit Level & RAM Story Counts do not match.
            }
            return allSpacingsAreEqual; // Revit Level & RAM Story Counts do match, returns whether or not spacings of all levels match or not.
        }

        public static void PerformLevelSpacingMappingEqualStoryCount(RAMModel ramModel, AnalyticalModel revitModel, Dictionary<int, string> levelSpacingMappingDict)
        {
            for (int i = 0; i < revitModel.LevelInfo.LevelsRevitSpacings.Count; i++)
            {
                levelSpacingMappingDict[revitModel.LevelInfo.Levels[i].ElementId] = ramModel.Stories[i].LayoutType;
            }
        }

        public static List<int> GenerateAllIndexesList(int storyCount)
        {
            var allIndexesList = new List<int>();
            for (int i = 0; i < storyCount; i++)
            {
                allIndexesList.Add(i);
            }
            return allIndexesList;
        }

        public static List<int> GenerateIndexesIteratedOverList(int indexOffset, int minStoryCount)
        {
            var indexesIteratedOver = new List<int>();
            for (int i = 0; i < minStoryCount; i++)
            {
                indexesIteratedOver.Add(indexOffset+i);
            }
            return indexesIteratedOver;
        }

        public static void FilterLevelStoryData(RAMModel ramModel, AnalyticalModel revitModel)
        {
            var levelInfo = revitModel.LevelInfo;
            double errorTolerance = levelInfo.LevelsRevitSpacings.Count * 1.0; // Allow a 1 foot discrepancy at each level.

            if (levelInfo.LevelsRevitSpacings.Count < ramModel.Stories.Count) // Not all RAM Levels will be mapped.
            {
                var indexesToRemoveToTotalErrorDict = new Dictionary<List<int>, double>();
                var potentialIndexesToRemoveList = new List<int>();
                var allIndexesList = GenerateAllIndexesList(ramModel.Stories.Count);
                var indexesIteratedOver = new List<int>();
                int numStoriesToRemove = ramModel.Stories.Count - levelInfo.LevelsRevitSpacings.Count;
                int numTotalErrorIterations = ramModel.Stories.Count - levelInfo.LevelsRevitSpacings.Count + 1;
                for (int i = 0; i < numTotalErrorIterations; i++)
                {
                    indexesIteratedOver = GenerateIndexesIteratedOverList(i, levelInfo.LevelsRevitSpacings.Count);
                    double totalError = CompareSpacingDiscrepancyAllLevels(ramModel, levelInfo, i);
                    potentialIndexesToRemoveList = allIndexesList.Except(indexesIteratedOver).ToList();

                    indexesToRemoveToTotalErrorDict[potentialIndexesToRemoveList] = totalError;
                }
                //FilterStoryList(ramModel.Stories);
                int numSolutions = 0;
                var indexesToRemove = new List<int>();
                foreach (var key in indexesToRemoveToTotalErrorDict.Keys)
                {
                    double totalError = indexesToRemoveToTotalErrorDict[key];
                    if (totalError < errorTolerance)
                    {
                        numSolutions += 1;
                        indexesToRemove = key;
                    }
                }
                if (numSolutions > 1)
                {
                    throw new Exception("More than one Level Spacing Mapping solution possible");
                }
                if (numSolutions == 0)
                {
                    throw new Exception("No Level Spacing Mapping soluton found");
                }

                for (int i = 0; i < indexesToRemove.Count; i++)
                {
                    ramModel.Stories.RemoveAt(i);
                }
                RegenerateRAMElevations(ramModel.Stories);
            }

            else if (levelInfo.LevelsRevitSpacings.Count > ramModel.Stories.Count) // Not all Revit Levels will be mapped.
            {
                // TODO: Remove extra revit Levels.
                // Only consider levels with beams hosted on them.
                var revitLevelsWithBeams = GetRevitLevelsWithBeams(revitModel);
                revitModel.LevelInfo.Levels = revitLevelsWithBeams;

                var indexesToRemoveToTotalErrorDict = new Dictionary<List<int>, double>();
                var potentialIndexesToRemoveList = new List<int>();
                var allIndexesList = GenerateAllIndexesList(ramModel.Stories.Count);
                var indexesIteratedOver = new List<int>();
                int numStoriesToRemove = Math.Abs(ramModel.Stories.Count - levelInfo.LevelsRevitSpacings.Count);
                int numTotalErrorIterations = numStoriesToRemove + 1;
                for (int i = 0; i < numTotalErrorIterations; i++)
                {
                    indexesIteratedOver = GenerateIndexesIteratedOverList(i, levelInfo.LevelsRevitSpacings.Count);
                    double totalError = CompareSpacingDiscrepancyAllLevels(ramModel, levelInfo, i);
                    potentialIndexesToRemoveList = allIndexesList.Except(indexesIteratedOver).ToList();

                    indexesToRemoveToTotalErrorDict[potentialIndexesToRemoveList] = totalError;
                }
                int numSolutions = 0;
                var indexesToRemove = new List<int>();
                foreach (var key in indexesToRemoveToTotalErrorDict.Keys)
                {
                    double totalError = indexesToRemoveToTotalErrorDict[key];
                    if (totalError < errorTolerance)
                    {
                        numSolutions += 1;
                        indexesToRemove = key;
                    }
                }
                if (numSolutions > 1)
                {
                    throw new Exception("More than one Level Spacing Mapping solution possible");
                }
                if (numSolutions == 0)
                {
                    throw new Exception("No Level Spacing Mapping soluton found");
                }

                for (int i = 0; i < indexesToRemove.Count; i++)
                {
                    revitModel.LevelInfo.Levels.RemoveAt(i);
                }


            }
            else if (levelInfo.LevelsRevitSpacings.Count == ramModel.Stories.Count)
            {
                throw new Exception("Counts are equal");

            }
            else
            {
                throw new Exception("Invalid RAM and Revit Level Spacing Count Compaison");
            }
        }

        public static List<LevelFloor> GetRevitLevelsWithBeams(AnalyticalModel revitModel)
        {
            var levelIdsWithBeams = new List<int>();
            foreach (var revitBeam in revitModel.StructuralMembers.Beams)
            {
                if (!levelIdsWithBeams.Contains(revitBeam.ElementLevelId))
                {
                    levelIdsWithBeams.Add(revitBeam.ElementLevelId);
                }
            }
            var filteredRevitLevels = new List<LevelFloor>();
            foreach (var revitLevelIdWithBeams in levelIdsWithBeams)
            {
                var revitLevelWithBeams = revitModel.LevelInfo.Levels.First(item => item.ElementId == revitLevelIdWithBeams);
                filteredRevitLevels.Add(revitLevelWithBeams);
            }

            List<LevelFloor> sortedFilteredRevitLevels = filteredRevitLevels.OrderBy(o => o.Elevation).ToList();
            int levelNumber = 1;
            foreach(var level in sortedFilteredRevitLevels)
            {
                level.LevelNumber = levelNumber;
                levelNumber++;
            }


            return sortedFilteredRevitLevels;
        }

        public static void RegenerateRAMElevations(List<RAMModel.Story> ramStories)
        {
            double totalPreviousElevation = 0.0;
            // Reset all Elevations and level numbers.
            for (int i = 0; i < ramStories.Count; i++)
            {
                ramStories[i].Elevation = 0.0;
                ramStories[i].Level = i + 1;
            }

            for (int i=0; i< ramStories.Count; i++)
            {
                ramStories[i].Elevation = ramStories[i].Height + totalPreviousElevation;
                totalPreviousElevation = ramStories[i].Elevation;
            }
        }

        public static double CompareSpacingDiscrepancyAllLevels(RAMModel ramModel, LevelInfo revitModelLevelInfo, int ramStartIndexOffset)
        {
            double totalError = 0.0;
            for (int i = 0; i < revitModelLevelInfo.Levels.Count-1; i++)
            {
                int revitBaseLevelNumber = revitModelLevelInfo.Levels[i].LevelNumber;
                int revitTopLevelNumber = revitModelLevelInfo.Levels[i + 1].LevelNumber;

                double revitLevelSpacing = revitModelLevelInfo.LevelsRevitSpacings[revitBaseLevelNumber.ToString() + '-' + revitTopLevelNumber.ToString()];
                double ramLevelSpacing = ramModel.Stories[i+ ramStartIndexOffset].Height;
                double spacingError = CompareLevelSpacingsDiscrepancy(ramLevelSpacing, revitLevelSpacing);
                totalError += spacingError;
            }
            return totalError;
        }

        public static void FilterStoryList(List<RAMModel.Story> ramStories)
        {
            var resultList = ramStories.RemoveAll(r => !r.MapRevitLevelToThis);
        }

        public static bool CompareLevelSpacings(double levelSpacingRAM, double levelSpacingRevit)
        {
            double epsilon = 1.0; // feet
            double deltaSpacing = Math.Abs(levelSpacingRAM - levelSpacingRevit);
            if (deltaSpacing < epsilon)
            {
                return true;
            }
            return false;
        }

        public static double CompareLevelSpacingsDiscrepancy(double levelSpacingRAM, double levelSpacingRevit)
        {
            double deltaSpacing = Math.Abs(levelSpacingRAM - levelSpacingRevit);
            return deltaSpacing;
        }

        public static bool IsLevelStoryCountMatching(int revitLevelCount, int ramStoryCount)
        {
            if(revitLevelCount - 1 == ramStoryCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CompareLevelNames(string levelNameRAM, string levelNameRevit)
        {
            bool levelNamesMatching = false;
            var levelNameRAMLowerCase = levelNameRAM.ToLower();
            var levelNameRevitLowerCase = levelNameRevit.ToLower();
            if (levelNameRAMLowerCase == levelNameRevitLowerCase)
            {
                levelNamesMatching = true;
                return levelNamesMatching;
            }
            if (levelNameRAMLowerCase.Contains(levelNameRevitLowerCase)) 
            {
                levelNamesMatching = true;
                return levelNamesMatching;
            }
            if (levelNameRevitLowerCase.Contains(levelNameRAMLowerCase))
            {
                levelNamesMatching = true;
                return levelNamesMatching;
            }
            return false;
        }

        public static void PerformLevelNameMapping(RAMModel.Story levelRAM, LevelFloor levelRevit, Dictionary<int, string> levelMappingDict)
        {
            bool doMapping = ModelCompare.CompareLevelNames(levelRAM.StoryLabel, levelRevit.Name);
            if (doMapping)
            {
                levelMappingDict[levelRevit.ElementId] = levelRAM.LayoutType;
            }
            else
            {
                return;
            }
        }


        public static bool CompareLevelElevations(double levelElevationRAM, double levelElevationRevit, double revitBaseLevelReference)
        {
            double epsilon = 1.0; // feet
            double deltaElevation = Math.Abs((levelElevationRAM + revitBaseLevelReference) - levelElevationRevit);
            if (deltaElevation < epsilon)
            {
                return true;
            }
            return false;
        }

        public static void PerformLevelElevationMapping(RAMModel.Story levelRAM, LevelFloor levelRevit, Dictionary<int, string> levelMappingDict, double revitBaseLevelReference)
        {
            bool doMapping = ModelCompare.CompareLevelElevations(levelRAM.Elevation, levelRevit.Elevation, revitBaseLevelReference);
            if (doMapping)
            {
                levelMappingDict[levelRevit.ElementId] = levelRAM.LayoutType;
            }
            else
            {
                return;
            }
        }

        public static void PerformLevelOrderMapping(List<RAMModel.Story> levelsRAM, List<LevelFloor> levelsRevit, Dictionary<int, string> levelMappingDict)
        {
            if (levelsRAM.Count == levelsRevit.Count)
            {
                List<RAMModel.Story> levelsRAMSorted = levelsRAM.OrderBy(o => o.Elevation).ToList();
                List<LevelFloor> levelsRevitSorted = levelsRevit.OrderBy(p => p.Elevation).ToList();
                for (int i = 0; i < levelsRevit.Count; i++)
                {
                    levelMappingDict[levelsRevit[i].ElementId] = levelsRAM[i].LayoutType;
                }
            }
            else
            {
                for (int i = 0; i < levelsRevit.Count; i++)
                {
                    levelMappingDict[levelsRevit[i].ElementId] = "None";
                }
            }
        }

        public static void PerformLevelMapping(List<RAMModel.Story> levelsRAM, LevelInfo levelInfoRevit, Dictionary<int, string> levelNameMappingDict, Dictionary<int, string> levelElevationMappingDict, Dictionary<int, string> levelOrderMappingDict, Dictionary<int, string> levelMappingDict, Dictionary<int, string> levelSpacingMappingDict, double revitBaseLevelReference)
        {
            var levelsRevit = levelInfoRevit.Levels;
            PerformLevelOrderMapping(levelsRAM, levelsRevit, levelOrderMappingDict);
            for (int i = 0; i < levelsRevit.Count; i++)
            {
                for (int j = 0; j < levelsRAM.Count; j++)
                {
                    ModelCompare.PerformLevelNameMapping(levelsRAM[j], levelsRevit[i], levelNameMappingDict);
                }
            }

            for (int i = 0; i < levelsRevit.Count; i++)
            {
                for (int j = 0; j < levelsRAM.Count; j++)
                {
                    PerformLevelElevationMapping(levelsRAM[j], levelsRevit[i], levelElevationMappingDict, revitBaseLevelReference);
                }
            }

            ModelCompare.PerformLevelMappingCrossChecking(levelNameMappingDict, levelElevationMappingDict, levelOrderMappingDict, levelMappingDict, levelsRevit, levelSpacingMappingDict);
        }

        public static void PerformLevelMappingCrossChecking(Dictionary<int, string> levelNameMappingDict, Dictionary<int, string> levelElevationMappingDict, Dictionary<int, string> levelOrderMappingDict, Dictionary<int, string> levelMappingDict, List<LevelFloor> levelsRevit, Dictionary<int, string> levelSpacingMappingDict)
        {
            for (int i = 0; i < levelsRevit.Count; i++)
            {
                int revitLevelId = levelsRevit[i].ElementId;
                string layoutTypeByName = levelNameMappingDict[revitLevelId];
                string layoutTypeByElevation = levelElevationMappingDict[revitLevelId];
                string layoutTypeByOrder = levelOrderMappingDict[revitLevelId];
                //string layoutTypeBySpacing = levelSpacingMappingDict[revitLevelId];

                var allEqual = new[] { layoutTypeByName, layoutTypeByElevation, layoutTypeByOrder }.Distinct().Count() == 1;
                var allNotEqual = new[] { layoutTypeByName, layoutTypeByElevation, layoutTypeByOrder }.Distinct().Count() == 3;

                if (allEqual)
                {
                    levelsRevit[i].MappingConfidence = 1;
                    levelMappingDict[revitLevelId] = layoutTypeByName;
                }
                else if((layoutTypeByName == layoutTypeByElevation))
                {
                    levelsRevit[i].MappingConfidence = 2;
                    levelMappingDict[revitLevelId] = layoutTypeByElevation;
                }
                else if ((layoutTypeByName == layoutTypeByOrder))
                {
                    levelsRevit[i].MappingConfidence = 3;
                    levelMappingDict[revitLevelId] = layoutTypeByName;
                }
                else if(allNotEqual)
                {
                    levelsRevit[i].MappingConfidence = 4;
                    levelMappingDict[revitLevelId] = layoutTypeByElevation;
                }

                else if (allNotEqual)
                {
                    levelsRevit[i].MappingConfidence = 5;
                    levelMappingDict[revitLevelId] = layoutTypeByOrder;
                }
                else
                {
                    throw new Exception("Unable to map levels. RAM model levels are different from Revit model levels");
                }

            }
        }

        // GRID MAPPING

        public static void ClassifyGridNameType(Grid grid)
        {
            Char gridNameDelimiter = '.';
            string gridName = grid.Name;
            bool gridNameContainsPeriod = gridName.Contains(gridNameDelimiter);
            if (gridNameContainsPeriod)
            {
                string[] gridNameComponents = gridName.Split(gridNameDelimiter);
                int n;
                bool isNumeric = int.TryParse(gridNameComponents[0], out n);
                if (isNumeric)
                {
                    grid.GridTypeNaming = Grid.GridTypeNamingClassification.Numbered;
                }
                else
                {
                    grid.GridTypeNaming = Grid.GridTypeNamingClassification.Lettered;
                }
            }
            else
            {
                int n;
                bool isNumeric = int.TryParse(gridName, out n);
                if (isNumeric)
                {
                    grid.GridTypeNaming = Grid.GridTypeNamingClassification.Numbered;
                }
                else
                {
                    grid.GridTypeNaming = Grid.GridTypeNamingClassification.Lettered;
                }
            }

        }
        public static void ClassifyGridDirectionalities(List<Grid> grids)
        {
            var letteredGrids = new List<Grid>();
            var numberedGrids = new List<Grid>();

            foreach (var grid in grids)
            {
                if (grid.GridTypeNaming == Grid.GridTypeNamingClassification.Lettered)
                {
                    letteredGrids.Add(grid);
                }
                else if (grid.GridTypeNaming == Grid.GridTypeNamingClassification.Numbered)
                {
                    numberedGrids.Add(grid);
                }
                else
                {
                    throw new Exception("Grid does not have proper GridTypeClassification");
                }
            }

            ClassifyGridDirectionality(letteredGrids, "A", "B");
            ClassifyGridDirectionality(numberedGrids, "1", "2");

        }

        public static void ClassifyGridDirectionality(List<Grid> grids, string gridName1, string gridName2)
        {
            int coordinateInt;
            if (grids[0].GridOrientation == GridOrientationClassification.Horizontal)
            {
                coordinateInt = 1;
            }
            else if (grids[0].GridOrientation == GridOrientationClassification.Vertical)
            {
                coordinateInt = 0;

            }
            else
            {
                throw new Exception("Grid Orientation has not been assigned or other case error.");
            }
            if (grids.Count > 1)
            {
                var locationA = grids.First(item => item.Name == gridName1 || item.Name == "01").Origin[coordinateInt];
                var locationB = grids.First(item => item.Name == gridName2 || item.Name == "02").Origin[coordinateInt];
                if (locationA < locationB)
                {
                    foreach (var grid in grids)
                    {
                        grid.DirectionalityClassification = Grid.GridDirectionalityClassification.Increasing;
                    }
                }
                else if (locationA > locationB)
                {
                    foreach (var grid in grids)
                    {
                        grid.DirectionalityClassification = Grid.GridDirectionalityClassification.Decreasing;
                    }
                }
                else
                {
                    throw new Exception("Comparing overlapping Grids error");
                }
            }
            else
            {
                throw new Exception("Only 1 RAM grid in that direction found error");
            }
        }

        public static Dictionary<Beam.BeamOrientationRelativeToGrid, double> DetermineTolerances(GridData gridData, List<RAMModel.RAMGrid> horizontalRAMGrids, List<RAMModel.RAMGrid> verticalRAMGrids)
        {
            var tolerances = new Dictionary<Beam.BeamOrientationRelativeToGrid, double>();
            // Measure Revit maximum grid to grid spacing Horizontal.
            double revitMaxVerticalSpacing = Math.Abs(DetermineRevitMaximumGridToGridSpacing(gridData.VerticalGridSpacings)*12.0); // feet to inches.
            // Measure Revit maximum grid to grid spacing Vertical.
            double revitMaxHorizontalSpacing = Math.Abs(DetermineRevitMaximumGridToGridSpacing(gridData.HorizontalGridSpacings)*12.0); // feet to inches.

            // Measure RAM maximum grid to grid spacing Horizontal.
            double ramMaxVerticalSpacing = Math.Abs(DetermineRAMMaximumGridToGridSpacing(horizontalRAMGrids));
            // Measure RAM maximum grid to grid spacing Vertical.
            double ramMaxHorizontalSpacing = Math.Abs(DetermineRAMMaximumGridToGridSpacing(verticalRAMGrids));

            // Calculate Horizontal Tolerance.
            double horizontalTolerance = Math.Abs(revitMaxHorizontalSpacing - ramMaxHorizontalSpacing);
            // Calculate Vertical Tolerance.
            double verticalTolerance = Math.Abs(revitMaxVerticalSpacing - ramMaxVerticalSpacing);

            tolerances[Beam.BeamOrientationRelativeToGrid.ParallelToHorizontalGrids] = horizontalTolerance;
            tolerances[Beam.BeamOrientationRelativeToGrid.ParallelToVerticalGrids] = verticalTolerance;

            return tolerances;


        }

        public static double DetermineRAMMaximumGridToGridSpacing(List<RAMModel.RAMGrid> ramGrids)
        {
            double maximumSpacing = 0.0;
            List<RAMModel.RAMGrid> sortedGrids = ramGrids.OrderBy(o => o.Location).ToList();
            maximumSpacing = sortedGrids.First().Location - sortedGrids.Last().Location;
            return maximumSpacing;
        }

        public static double DetermineRevitMaximumGridToGridSpacing(Dictionary<string, double> gridSpacingsDict)
        {
            double maximumSpacing = 0.0;

            foreach (KeyValuePair<string, double> entry in gridSpacingsDict)
            {
                maximumSpacing += entry.Value;
            }
            return maximumSpacing;
        }

        public static string GetRevitLevelNameFromId(int levelId, List<LevelFloor> revitLevels)
        {
            return revitLevels.First(item => item.ElementId == levelId).Name;
        }

        public static int GetRevitLevelIdFromRAMLayoutType(Dictionary<int, string> levelMappingDict, string ramLayoutType)
        {
            return levelMappingDict.FirstOrDefault(x => "Floor Type: " + x.Value == ramLayoutType).Key;

        }
        // BEAM MAPPING.

        public static Results PerformBeamMapping(RAMModel ramModel, AnalyticalModel revitModel, Dictionary<int, string> levelMappingDict, Results results, double tolerance)
        {
            Dictionary<string, string> beamRevitMappingByLevelResults = new Dictionary<string, string>();
            Dictionary<string, string> beamRamMappingByLevelResults = new Dictionary<string, string>();
            Dictionary<string, string> revitLevelToRAMLevelMappingResults = new Dictionary<string, string>();

            int numMappedBeamsTotal = 0;
            // Offset RAM Beam X & Y based on Reference Point.
            var offset = MapReferencePoints(ramModel.ReferencePointDataTransfer, revitModel.ReferencePointDataTransfer);
            var directionalityFactors = DetermineDirectionalityFactors(revitModel.GridData, ramModel.Grids);
            OffsetRamBeamCoordinates(ramModel.RamBeams, offset, directionalityFactors);

            var ramBeamToLayoutMapping = GenerateRAMBeamToLayoutMapping(ramModel.RamBeams);
            var revitBeamToLayoutMapping = GenerateRevitBeamToLayoutMapping(revitModel.StructuralMembers.Beams, levelMappingDict);
            List<RAMModel.RAMBeam> ramBeamList = new List<RAMModel.RAMBeam>();
            List<Beam> revitBeamList = new List<Beam>();
            // Loop over RAM Layout Type Keys (Layout Type).
            Dictionary<string, int> numMappedBeamsPerFloor = new Dictionary<string, int>();
            Dictionary<string, double> revitBeamTolerancesForMappingSuccess = new Dictionary<string, double>();

            foreach (var layoutType in ramBeamToLayoutMapping.Keys)
            {
                numMappedBeamsPerFloor[layoutType] = 0;
            }
                while (tolerance < 48.0)
            {
                foreach (var layoutType in ramBeamToLayoutMapping.Keys)
                {
                    //int numMappedBeamsPerFloor = 0;

                    int revitLevelId = GetRevitLevelIdFromRAMLayoutType(levelMappingDict, layoutType);
                    string revitLevelName = GetRevitLevelNameFromId(revitLevelId, revitModel.LevelInfo.Levels);
                    revitLevelToRAMLevelMappingResults[revitLevelName] = layoutType;
                    ramBeamList = ramBeamToLayoutMapping[layoutType];
                    revitBeamList = revitBeamToLayoutMapping[layoutType];
                    revitBeamList = FilterOutNonRAMBeamsFromRevitBeamList(revitBeamList);
                    revitBeamList = FilterRevitBeamListByType(revitBeamList);
                    // Compare RAM Beam and Revit Beam Start & End X & Y Coordinates.
                    // If coordinates matching then assign Revit Beam with coresponding reactions.
                    foreach (var ramBeam in ramBeamList)
                    {
                        foreach (var revitBeam in revitBeamList)
                        {
                            if (revitBeam.IsMappedToRAMBeam)
                            {
                                continue;
                            }
                            if (ComparePoints(ramBeam.StartPoint, revitBeam.StartPoint, tolerance) && ComparePoints(ramBeam.EndPoint, revitBeam.EndPoint, tolerance))
                            {
                                revitBeam.StartReactionTotal = ramBeam.StartTotalReactionPositive.ToString();
                                revitBeam.EndReactionTotal = ramBeam.EndTotalReactionPositive.ToString();
                                numMappedBeamsPerFloor[layoutType] += 1;
                                revitBeam.IsMappedToRAMBeam = true;
                                revitBeam.ToleranceForSuccessFulMapping = tolerance;
                                revitBeamTolerancesForMappingSuccess[revitBeam.ElementId.ToString()] = tolerance;
                                ramBeam.IsMappedToRevitBeam = true;
                                //continue;
                            }
                            if (ComparePoints(ramBeam.StartPoint, revitBeam.EndPoint, tolerance) && ComparePoints(ramBeam.EndPoint, revitBeam.StartPoint, tolerance))
                            {
                                revitBeam.StartReactionTotal = ramBeam.EndTotalReactionPositive.ToString();
                                revitBeam.EndReactionTotal = ramBeam.StartTotalReactionPositive.ToString();
                                numMappedBeamsPerFloor[layoutType] += 1;
                                revitBeam.IsMappedToRAMBeam = true;
                                revitBeam.ToleranceForSuccessFulMapping = tolerance;
                                revitBeamTolerancesForMappingSuccess[revitBeam.ElementId.ToString()] = tolerance;
                                ramBeam.IsMappedToRevitBeam = true;
                                //continue;
                            }
                        }
                    }
                    string proportionRamBeamsMappedPerFloor = numMappedBeamsPerFloor[layoutType].ToString() + "/" + ramBeamList.Count.ToString();
                    string proportionRevitBeamsMappedPerFloor = numMappedBeamsPerFloor[layoutType].ToString() + "/" + revitBeamList.Count.ToString();
                    beamRamMappingByLevelResults[layoutType] = proportionRamBeamsMappedPerFloor;
                    beamRevitMappingByLevelResults[revitLevelName] = proportionRevitBeamsMappedPerFloor;
                    numMappedBeamsTotal = numMappedBeamsPerFloor.Sum(x => x.Value);
                }
                tolerance += 4.0;
            }

            // Populate results.
            results.RAMMappingResultsByFloor = beamRamMappingByLevelResults;
            results.RevitMappingResultsByFloor = beamRevitMappingByLevelResults;
            results.RevitLevelToRAMFloorLayoutTypeMapping = revitLevelToRAMLevelMappingResults;
            results.TotalMappedBeamCount = numMappedBeamsTotal;
            results.RevitBeamTolerancesForMappingSuccess = revitBeamTolerancesForMappingSuccess;
            return results;
        }

        // Generate RAM Layout Type to RAM Beam Mapping.
        public static Dictionary<string, List<RAMModel.RAMBeam>> GenerateRAMBeamToLayoutMapping(List<RAMModel.RAMBeam> ramBeams)
        {
            Dictionary<string, List<RAMModel.RAMBeam>> beamToLayoutMapping = new Dictionary<string, List<RAMModel.RAMBeam>>();
            List<RAMModel.RAMBeam> beamList = null;
            foreach (var beam in ramBeams)
            {
                var layoutType = beam.FloorLayoutType;

                if (!beamToLayoutMapping.TryGetValue(beam.FloorLayoutType, out beamList))
                    beamToLayoutMapping.Add(beam.FloorLayoutType, beamList = new List<RAMModel.RAMBeam>());
                beamList.Add(beam);
            }
            return beamToLayoutMapping;

        }

        // Generate Revit Layout Type to Revit Beam Mapping.
        public static Dictionary<string, List<Beam>> GenerateRevitBeamToLayoutMapping(List<Beam> revitBeams, Dictionary<int, string> levelMappingDict)
        {
            Dictionary<string, List<Beam>> beamToLayoutMapping = new Dictionary<string, List<Beam>>();
            List<Beam> beamList = null;
            foreach (var beam in revitBeams)
            {
                var beamRevitLevelId = beam.ElementLevelId;
                beam.RAMFloorLayoutType = levelMappingDict[beamRevitLevelId];

                if (!beamToLayoutMapping.TryGetValue("Floor Type: " + beam.RAMFloorLayoutType, out beamList))
                    beamToLayoutMapping.Add("Floor Type: " + beam.RAMFloorLayoutType, beamList = new List<Beam>());
                beamList.Add(beam);
            }
            return beamToLayoutMapping;

        }

        public static List<Beam> FilterRevitBeamListByType(List<Beam> revitBeamList)
        {
            List<Beam> filteredRevitBeamList = new List<Beam>();
            foreach (var beam in revitBeamList)
            {
                string section = beam.Section;
                char sectionSymbol = section[0];
                if(sectionSymbol == 'W')
                {
                    filteredRevitBeamList.Add(beam);
                }

            }
            return filteredRevitBeamList;
        }

        // Reference Point Mapping.
        public static double[] MapReferencePoints(double[] ramReferencePoint, double[] revitReferencePoint)
        {
            var offset = new double[3];
            //offset[1] = revitReferencePoint[0] - ramReferencePoint[0];
            //offset[0] = revitReferencePoint[1] - ramReferencePoint[1];
            offset[0] = revitReferencePoint[0] - ramReferencePoint[0];
            offset[1] = revitReferencePoint[1] - ramReferencePoint[1];
            offset[2] = revitReferencePoint[2] - ramReferencePoint[2];
            return offset;
        }

        // Offset RAM Beam X & Y based on Reference Point.
        public static void OffsetRamBeamCoordinates(List<RAMModel.RAMBeam> ramBeams, double[] offset, int[] directionalityFactors)
        {
            foreach (var beam in ramBeams)
            {
                beam.StartPoint[1] += offset[1];
                beam.StartPoint[0] += offset[0];
                //beam.StartPoint[2] += offset[2];

                beam.EndPoint[1] += offset[1];
                beam.EndPoint[0] += offset[0];
               //beam.EndPoint[2] += offset[2];
            }
        }

        public static int[] DetermineDirectionalityFactors(GridData revitGridData, List<RAMModel.RAMGrid> ramGrids)
        {
            var factors = new int[2]; // 0 --> lettered. 1 --> numbered.
            Grid revitGridLettered = revitGridData.Grids.Where(i => i.GridTypeNaming == Grid.GridTypeNamingClassification.Lettered).FirstOrDefault();
            var revitGridLetteredDirectionality = revitGridLettered.DirectionalityClassification;
            Grid revitGridNumbered = revitGridData.Grids.Where(i => i.GridTypeNaming == Grid.GridTypeNamingClassification.Numbered).FirstOrDefault();
            var revitGridNumberedDirectionality = revitGridNumbered.DirectionalityClassification;

            RAMModel.RAMGrid ramGridLettered = ramGrids.Where(i => i.GridTypeNaming == RAMModel.RAMGrid.GridTypeNamingClassification.Lettered).FirstOrDefault();
            var ramGridLetteredDirectionality = ramGridLettered.DirectionalityClassification;
            RAMModel.RAMGrid ramGridNumbered = ramGrids.Where(i => i.GridTypeNaming == RAMModel.RAMGrid.GridTypeNamingClassification.Numbered).FirstOrDefault();
            var ramGridNumberedDirectionality = ramGridNumbered.DirectionalityClassification;

            if(revitGridLetteredDirectionality == Grid.GridDirectionalityClassification.Increasing && ramGridLetteredDirectionality == RAMModel.GridDirectionalityClassification.Increasing)
            {
                factors[0] = 1;
            }
            else if (revitGridLetteredDirectionality == Grid.GridDirectionalityClassification.Decreasing && ramGridLetteredDirectionality == RAMModel.GridDirectionalityClassification.Decreasing)
            {
                factors[0] = 1;
            }
            else if (revitGridLetteredDirectionality == Grid.GridDirectionalityClassification.Increasing && ramGridLetteredDirectionality == RAMModel.GridDirectionalityClassification.Decreasing)
            {
                factors[0] = -1;
            }
            else if (revitGridLetteredDirectionality == Grid.GridDirectionalityClassification.Decreasing && ramGridLetteredDirectionality == RAMModel.GridDirectionalityClassification.Increasing)
            {
                factors[0] = -1;
            }
            else if (revitGridLetteredDirectionality == Grid.GridDirectionalityClassification.None || ramGridLetteredDirectionality == RAMModel.GridDirectionalityClassification.None)
            {
                throw new Exception("Grid Directionality not classfified");
            }
            else
            {
                throw new Exception("Unrecognized grid classification combination");
            }



            if (revitGridNumberedDirectionality == Grid.GridDirectionalityClassification.Increasing && ramGridNumberedDirectionality == RAMModel.GridDirectionalityClassification.Increasing)
            {
                factors[1] = 1;
            }
            else if (revitGridNumberedDirectionality == Grid.GridDirectionalityClassification.Decreasing && ramGridNumberedDirectionality == RAMModel.GridDirectionalityClassification.Decreasing)
            {
                factors[1] = 1;
            }
            else if (revitGridNumberedDirectionality == Grid.GridDirectionalityClassification.Increasing && ramGridNumberedDirectionality == RAMModel.GridDirectionalityClassification.Decreasing)
            {
                factors[1] = -1;
            }
            else if (revitGridNumberedDirectionality == Grid.GridDirectionalityClassification.Decreasing && ramGridNumberedDirectionality == RAMModel.GridDirectionalityClassification.Increasing)
            {
                factors[1] = -1;
            }
            else if (revitGridNumberedDirectionality == Grid.GridDirectionalityClassification.None || ramGridNumberedDirectionality == RAMModel.GridDirectionalityClassification.None)
            {
                throw new Exception("Grid Directionality not classfified");
            }
            else
            {
                throw new Exception("Unrecognized grid classification combination");
            }
            return factors;
        }

        public static bool ComparePoints(double[] point1, double[] point2, double tolerance)
        {
            //double toleranceX = tolerances[Beam.BeamOrientationRelativeToGrid.ParallelToVerticalGrids];
            //double toleranceY = tolerances[Beam.BeamOrientationRelativeToGrid.ParallelToHorizontalGrids];
            //double miniumTolerance = 14.0; // inches.
            //double bufferTolerance = 2.0; // inches.
            //toleranceY = Math.Max(miniumTolerance, toleranceY);
            //toleranceX = Math.Max(miniumTolerance, toleranceX);
            //toleranceY = 24.0;
            //toleranceX = 24.0;
            if (Math.Abs(point1[0] - point2[0]) < tolerance && Math.Abs(point1[1] - point2[1]) < tolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<Beam>FilterOutNonRAMBeamsFromRevitBeamList(List<Beam> revitBeams)
        {
            List<Beam> revitBeamsToRemove = new List<Beam>();
            var deepestBeam = revitBeams.OrderByDescending(i => i.Depth).FirstOrDefault();
            double maxDepth = deepestBeam.Depth; // inches.
            double tolerance = 2.0 * maxDepth;

            foreach (var revitBeam in revitBeams)
            {
                double zOffsetValueMagnitude = Math.Abs(revitBeam.ZOffsetValue); // inches.
                double startLevelOffsetMagnitude = Math.Abs(revitBeam.StartLevelOffset);
                double endLevelOffsetMagnitude = Math.Abs(revitBeam.EndLevelOffset);

                if (zOffsetValueMagnitude > tolerance || startLevelOffsetMagnitude > tolerance || endLevelOffsetMagnitude > tolerance )
                {
                    revitBeamsToRemove.Add(revitBeam);
                }
                else
                {
                    continue;
                }
            }
            var filteredRevitBeamList = revitBeams.Except(revitBeamsToRemove).ToList();

            return filteredRevitBeamList;
        }



    }
}
