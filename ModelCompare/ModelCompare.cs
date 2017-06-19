﻿using System;
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

        public static void CompareModels(RAMModel ramModel, AnalyticalModel revitModel)
        {
            var modelCompare = new ModelCompare(ramModel, revitModel);
            // Grid Mapping.
            foreach (var revitGrid in revitModel.GridData.Grids)
            {
                ClassifyGridNameType(revitGrid);

            }
            ClassifyGridDirectionalities(revitModel.GridData.Grids);
            FilterLevelStoryData(ramModel, revitModel);
            PerformLevelMapping(ramModel.Stories, revitModel.LevelInfo, modelCompare.LevelNameMapping, modelCompare.LevelElevationMapping, modelCompare.LevelOrderMapping, modelCompare.LevelMapping, modelCompare.LevelSpacingMapping, revitModel.LevelInfo.BaseReferenceElevation);
        }

        // LEVEL MAPPING.

        public static void FilterLevelStoryData(RAMModel ramModel, AnalyticalModel revitModel)
        {
            var levelInfo = revitModel.LevelInfo;
            // if story counts are matching and all spacings are almost equal then perform mapping
            bool levelStoryCountsMatch = IsLevelStoryCountMatching(revitModel.LevelInfo.LevelCount, ramModel.StoryCount);
            if (levelStoryCountsMatch)
            {
                for (int i = 0; i < levelInfo.LevelsRevitSpacings.Count; i++)
                {
                    int revitBaseLevelNumber = levelInfo.Levels[i].LevelNumber;
                    int revitTopLevelNumber = levelInfo.Levels[i + 1].LevelNumber;

                    double revitLevelSpacing = levelInfo.LevelsRevitSpacings[revitBaseLevelNumber.ToString() + '-' + revitTopLevelNumber.ToString()];
                    double ramLevelSpacing = ramModel.Stories[i].Height;
                    if (CompareLevelSpacings(ramLevelSpacing, revitLevelSpacing))
                    {
                        //levelMappingDict[levelInfo.Levels[i].ElementId] = ramModel.Stories[i].LayoutType;
                        ramModel.Stories[i].MapRevitLevelToThis = true;

                    }
                    else
                    {
                        ramModel.Stories[i].MapRevitLevelToThis = false;
                    }
                    // TODO: Check if spacing = sum of spacings of more than 1 level.

                }
            }
            else if(levelInfo.LevelsRevitSpacings.Count < ramModel.Stories.Count) // Not all RAM Levels will be mapped.
            {
                for (int i = 0; i < levelInfo.LevelsRevitSpacings.Count; i++)
                {
                    int revitBaseLevelNumber = levelInfo.Levels[i].LevelNumber;
                    int revitTopLevelNumber = levelInfo.Levels[i + 1].LevelNumber;

                    double revitLevelSpacing = levelInfo.LevelsRevitSpacings[revitBaseLevelNumber.ToString() + '-' + revitTopLevelNumber.ToString()];
                    double ramLevelSpacing = ramModel.Stories[i].Height;
                    if (CompareLevelSpacings(ramLevelSpacing, revitLevelSpacing))
                    {
                        //levelMappingDict[levelInfo.Levels[i].ElementId] = ramModel.Stories[i].LayoutType;
                        ramModel.Stories[i].MapRevitLevelToThis = true;
                    }
                    else
                    {
                        ramModel.Stories[i].MapRevitLevelToThis = false;
                    }
                    // TODO: Check if spacing = sum of spacings of more than 1 level.

                }
                FilterStoryList(ramModel.Stories);

            }

            else if(levelInfo.LevelsRevitSpacings.Count > ramModel.Stories.Count) // Not all Revit Levels will be mapped.
            {

            }
            else
            {
                throw new Exception("Invalid RAM and Revit Level Spacing Count Compaison");
            }



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

        //public static void PerformLevelSpacingMappingEqualStoryCount(RAMModel.Story levelRAM, LevelFloor levelRevit, Dictionary<int, string> levelMappingDict)
        //{
        //}

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

            ModelCompare.PerformLevelMappingCrossChecking(levelNameMappingDict, levelElevationMappingDict, levelOrderMappingDict, levelMappingDict, levelsRevit);
        }

        public static void PerformLevelMappingCrossChecking(Dictionary<int, string> levelNameMappingDict, Dictionary<int, string> levelElevationMappingDict, Dictionary<int, string> levelOrderMappingDict, Dictionary<int, string> levelMappingDict, List<LevelFloor> levelsRevit)
        {
            for (int i = 0; i < levelsRevit.Count; i++)
            {
                int revitLevelId = levelsRevit[i].ElementId;
                string layoutTypeByName = levelNameMappingDict[revitLevelId];
                string layoutTypeByElevation = levelElevationMappingDict[revitLevelId];
                string layoutTypeByOrder = levelOrderMappingDict[revitLevelId];
                var allEqual = new[] { layoutTypeByName, layoutTypeByElevation, layoutTypeByOrder }.Distinct().Count() == 1;
                var allNotEqual = new[] { layoutTypeByName, layoutTypeByElevation, layoutTypeByOrder }.Distinct().Count() == 3;

                if (allEqual && layoutTypeByName != "None")
                {
                    levelsRevit[i].MappingConfidence = 1;
                    levelMappingDict[revitLevelId] = layoutTypeByName;
                }
                else if((layoutTypeByName == layoutTypeByElevation) &&  (layoutTypeByElevation != "None"))
                {
                    levelsRevit[i].MappingConfidence = 2;
                    levelMappingDict[revitLevelId] = layoutTypeByElevation;
                }
                else if ((layoutTypeByName == layoutTypeByOrder) && (layoutTypeByName != "None"))
                {
                    levelsRevit[i].MappingConfidence = 3;
                    levelMappingDict[revitLevelId] = layoutTypeByName;
                }
                else if(allNotEqual && (layoutTypeByElevation != "None"))
                {
                    levelsRevit[i].MappingConfidence = 4;
                    levelMappingDict[revitLevelId] = layoutTypeByElevation;
                }

                else if (allNotEqual && (layoutTypeByElevation == "None"))
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
                var locationA = grids.First(item => item.Name == gridName1).Origin[coordinateInt];
                var locationB = grids.First(item => item.Name == gridName2).Origin[coordinateInt];
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


        // BEAM MAPPING.

        public static void PerformBeamMapping()
        {

        }
    }
}
