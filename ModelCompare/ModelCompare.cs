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
            PerformLevelMapping(ramModel.Stories, revitModel.LevelInfo, modelCompare.LevelNameMapping, modelCompare.LevelElevationMapping, modelCompare.LevelOrderMapping, modelCompare.LevelMapping, revitModel.LevelInfo.BaseReferenceElevation);
        }

        public static void FilterLevelStoryData(RAMModel ramModel, AnalyticalModel revitModel, Dictionary<int, string> levelMappingDict)
        {
            var levelInfo = revitModel.LevelInfo;
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
                        levelMappingDict[levelInfo.Levels[i].ElementId] = ramModel.Stories[i].LayoutType;
                    }
                    else
                    {
                        ramModel.Stories[i].MapRevitLevelToThis = false;
                        levelMappingDict[levelInfo.Levels[i].ElementId] = "None";
                    }
                    // TODO: Check if spacing = sum of spacings of more than 1 level.

                }
            }
            else if(levelInfo.LevelsRevitSpacings.Count < ramModel.Stories.Count) // Not all RAM Levels will be mapped.
            {

            }

            else if(levelInfo.LevelsRevitSpacings.Count > ramModel.Stories.Count) // Not all Revit Levels will be mapped.
            {

            }
            else
            {
                throw new Exception("Invalid RAM and Revit Level Spacing Count Compaison");
            }

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

        public static void PerformLevelMapping(List<RAMModel.Story> levelsRAM, LevelInfo levelInfoRevit, Dictionary<int, string> levelNameMappingDict, Dictionary<int, string> levelElevationMappingDict, Dictionary<int, string> levelOrderMappingDict, Dictionary<int, string> levelMappingDict, double revitBaseLevelReference)
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
    }
}
