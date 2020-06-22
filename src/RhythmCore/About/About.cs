using System;
using System.Collections.Generic;
using System.IO;

namespace Rhythm.About
{
    /// <summary>
    /// Wrapper for about class.
    /// </summary>
    public class About
    {
        private About()
        {
        }

        /// <summary>
        /// This is mostly to show the icon in the Dynamo 2.0 library.
        /// </summary>
        /// <returns></returns>
        public static string AboutRhythm()
        {
            string executingLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string aboutRhythm = "Rhythm is a set of useful nodes to help your Revit project maintain a good rhythm with Dynamo.";


            List<string> quoteList = new List<string>
            {
                "\"Never be limited by other people’s limited imaginations.\" - Dr. Mae Jemison",
                "\"Success is to be measured not so much by the position that one has reached in life as by the obstacles which he has overcome while trying to succeed.\" - Booker T. Washington",
                "\"I have not failed. I’ve just found 10,000 ways that won’t work.\" - Thomas A.Edison",
                "\"If you can dream, you can do it.\" - Walt Disney",
                "\"Where there is a will, there is a way. If there is a chance in a million that you can do something, anything, to keep what you want from ending, do it. Pry the door open or, if need be, wedge your foot in that door and keep it open.\" - Pauline Kael",
                "\"The future belongs to those who believe in the beauty of their dreams.\" - Eleanor Roosevelt",
                "\"You just can’t beat the person who never gives up.\" - Babe Ruth",
                "\"Start where you are. Use what you have. Do what you can.\" - Arthur Ashe",
                "\"If you can't explain it simply, you don't understand it well enough.\" - Albert Einstein",
                "\"No work is ever wasted. If it’s not working, let go and move on – it’ll come back around to be useful later.\" - Pixar"
            };


            Random rnd = new Random();
            int r = rnd.Next(quoteList.Count);

            return aboutRhythm + "\n\n" + quoteList[r];
        }

        private static void WritePackageJson(string filePath)
        {
            File.WriteAllText(filePath, "{\"license\":\"BSD 3-Clause\",\"file_hash\":null,\"name\":\"Rhythm\",\"version\":\"2019.7.24\",\"description\":\"Built against Dynamo 1.3.4/2.0.x and Revit 2017+. Rhythm is a set of useful nodes to help your Revit project maintain a good rhythm with Dynamo. Rhythm is authored by John Pierson (@60secondrevit) of Parallax Team (http://www.parallaxteam.com/) and sixtysecondrevit.com. \",\"group\":\"Breaking CAD\",\"keywords\":[\"documentation\",\"dimension\",\"curtainwall\",\"open\",\"document\",\"copy\",\"unopened\",\"background\"],\"dependencies\":[],\"contents\":\"All Upper - This converts the ase of selected items to UPPER. Inputs require booleans to enable for category., Arrowhead Assigner - This node assings the arrowhead type for all text and keynote tags. There is an input for ones you want to exclude., Collector.ModelGroupByName - This will collect all instances of a model group by given name., Collector.OfExteriorWalls - This node will collect all exterior (function) walls., Collector.OfInteriorWalls - This node collects interior (function) walls,, Collector.OfLegendViews - This node will collect all legend views in the project., Collector.OfStructuralWalls - This will collect only structural walls., Collector.PlacedRooms - This will collect placed rooms in current file., Isolated Pick Model Elements(ordered) - Pick model elements in desired order of a desired category., Isolated Pick Model Elements - This allows a multiple selection of elements of specified category., Rhythm.3DRoomTags - This node will generate 3D room tags for all rooms in the model. (RFA in extra folder in Dynamo download), Rhythm.RenumberRoomsByCurve - Complete revamp of the previous proposed workflows that break on rooms whose solids were irregular., Rhythm.WhatTheNode - This node will let you search dynamonodes.com for the most likely package a node is from. Helps fix \\\"red\\\" nodes when users do not have package installed., UI.ColorPicker - This node allows the user to choose a color via the Revit API color selection dialog. For Revit 2017 +\",\"engine_version\":\"2.0.3.8811\",\"engine\":\"dynamo\",\"engine_metadata\":\"\",\"site_url\":\"http://www.sixtysecondrevit.com\",\"repository_url\":\"https://github.com/sixtysecondrevit/RhythmForDynamo\",\"contains_binaries\":true,\"node_libraries\":[\"RhythmCore, Version=2019.9.10, Culture=neutral, PublicKeyToken=null\",\"RhythmRevit, Version=2019.9.10, Culture=neutral, PublicKeyToken=null\",\"RhythmUI, Version=2018.3.15.0, Culture=neutral, PublicKeyToken=null\"]}\r\n");
        }
    }
}
