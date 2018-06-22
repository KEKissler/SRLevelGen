using System;
using System.Collections.Generic;
using System.Text;

namespace SRLvlGen
{
    //creates a 3x2 matrix holding 4 segments that define the overall shape of the map to be generated
    public class LvlShapeGen
    {
        //   ┌────┐     ┌────┐
        //cw:│    v ccw:│    ^
        //   └────┘     └────┘
        private enum Segment {empty, cw, ccw};

        private Segment prefferedSegmentType, alternateSegmentType, forcedType;

        private BiasedRNG brng = new BiasedRNG();

        private List<int> unassignedSegments = new List<int>() { 0, 1, 2, 3, 4, 5 };

        private Segment[] map = new Segment[6]; // [0 1 2]
                                                // [3 4 5]
        private Dictionary<int, List<int>> adjacencyMap = new Dictionary<int, List<int>>();
        
        public LvlShapeGen(int seed = 0)
        {
            //creating the brng
            brng = new BiasedRNG(seed:seed);
            //filling adjacencyMap with the information for a 3x2 matrix shape
            adjacencyMap.Add(0, new List<int>() { 1, 3 });
            adjacencyMap.Add(1, new List<int>() { 0, 4, 2 });
            adjacencyMap.Add(2, new List<int>() { 1, 5 });
            adjacencyMap.Add(3, new List<int>() { 0, 4 });
            adjacencyMap.Add(4, new List<int>() { 1, 3, 5 });
            adjacencyMap.Add(5, new List<int>() { 2, 4 });
        }

        private void reset()
        {
            map = new Segment[6];
            unassignedSegments = new List<int>() { 0, 1, 2, 3, 4, 5 };
            //decide the preference direction, 50% chance
            prefferedSegmentType = (brng.nextDouble() > 0.5d) ? Segment.cw : Segment.ccw;
            alternateSegmentType = 3 - prefferedSegmentType;//preferred is 1 or 2, this is the other option
            //forced type is a workaround to create better circuts
            forcedType = Segment.empty;
        }

        public void genNewLevelShape(bool writeOutput = true, bool prettyPrint = false)
        {
            reset();//cleans up any previous generations
            //performs repeated actions until things are resolved. (Possibly repeating actions if a given action is invalid)
            while (unassignedSegments.Count > 2)
            {
                if(writeOutput)
                    Console.WriteLine("\nStarting a new action to create a Segment.");
                //action is to place a new segment into the current map
                //create a list of unused tiles that have adjacent existing segments, or if first action, any tile
                List<int> possibleLocations = new List<int>();
                if (unassignedSegments.Count == 6)
                {
                    if (writeOutput)
                        Console.WriteLine("First time through, possibleLocations is all [0-5].");
                    //if nothing has been placed, all locations are possible
                    possibleLocations = unassignedSegments;
                }
                else {
                    if (writeOutput)
                        Console.WriteLine("Starting to construct possibleLocations...\n");
                    //specifically only trying to check the validity of unassignedSegments that are adjacent to existing segments
                    
                    foreach (int i in unassignedSegments)
                    {
                        if (writeOutput)
                            Console.Write(i + " {");
                        bool hasAdjacentSegment = false;
                        //keeping track of the types of adjacent segments for further filtering
                        List<Segment> adjacentSegments = new List<Segment>();
                        foreach(int a in adjacencyMap[i])
                        {
                            //if an adjacent tile has been found to already exist
                            if (!unassignedSegments.Contains(a))
                            {
                                hasAdjacentSegment = true;
                                adjacentSegments.Add(map[a]);
                                if (writeOutput)
                                    Console.Write(a + ", ");
                            }
                        }
                        if (writeOutput)
                            Console.WriteLine("}");

                        //if, for this given int i, there are adjacent segments found, do further testing to see if should add
                        if (hasAdjacentSegment)
                        {
                            //in here, the goal is to filter out results that have multiple adjacent, but different type segments
                            //this has been proven to result in map making errors
                            bool allAdjacentSegmentsAreTheSame = true;
                            Segment first = adjacentSegments[0];
                            foreach(Segment s in adjacentSegments)
                            {
                                if(s != first)
                                {
                                    allAdjacentSegmentsAreTheSame = false;
                                }
                            }
                            if (allAdjacentSegmentsAreTheSame)
                            {
                                //works! add it.
                                possibleLocations.Add(i);
                                //another case that seems to make bad maps is if at this point adjacentSegments.Count > 2 and then
                                //the code below that creates a new segment chooses one that differs from both its adjacent
                                //so im making a forced choice overriding thing right here to solve that
                                if(adjacentSegments.Count == 2)
                                {
                                    forcedType = adjacentSegments[0];//both proven same at this point so copy the first
                                }

                            }
                        }
                    }
                }
                if (writeOutput)
                {
                    Console.Write("\tpossibleLocations = ");
                    foreach (int q in possibleLocations)
                        Console.Write(q + " ");
                    Console.WriteLine();
                }

                //now that possibleLocations has all empty tiles with adjacent existing tiles
                //randomly select one value from the possibleLocations to work with
                double randnum = brng.nextDouble();
                int positionSelection = (int)(Math.Floor(possibleLocations.Count * randnum));
                int selection = possibleLocations[positionSelection];
                //80% chance to generate a segment of the preferred type and 20% to generate one of the alternate type
                if (brng.nextDouble() < 0.8d)
                {
                    //preferred
                    map[selection] = prefferedSegmentType;
                }
                else
                {
                    //alternate
                    map[selection] = alternateSegmentType;
                }
                //forced generation override to fix an issue, if its empty, it doesnt change things, if it isnt it does that and then resets
                if(forcedType != Segment.empty)
                {
                    map[selection] = forcedType;
                    forcedType = Segment.empty;
                }

                //tell our unassignedSegments that we made another one
                unassignedSegments.Remove(selection);
                if (writeOutput)
                    Console.WriteLine("Generated a Segment at " + selection);
                   
            }
            if (writeOutput)
                Console.WriteLine("Success\nPrinting map...");
            int formatting = 3;
            foreach (Segment s in map)
            {
                Console.Write((int)s + " ");
                if (--formatting == 0)
                    Console.WriteLine();
            }
            if (prettyPrint)
            {
                //   ┌────┐     ┌────┐     ─\ /─        \    / 
                //cw:│    v ccw:│    ^       X            ><    
                //   └────┘     └────┘     ─/ \─        /    \ 



                //┌──────\ /──────┐ ┌──────┐
                //│       X       │ │      │
                //└──────/ \──────┘ └──────┘

                //0        1         2
                //12345678901234567890123456
                //┌──────┐ ┌──────┐ ┌──────┐
                //│      │ │      │ │      │
                //└\    /┘ └──────┘ └──────┘
                //   ><
                //┌/    \─────────\ /──────┐
                //│                X       ^
                //└───────────────/ \──────┘

                string output = "";
                //line 1/7

                //start of 0
                output += (map[0] != 0) ? "┌──────" : "       ";
                //upper connector between 0 and 1
                output += upperConnector((int)map[0], (int)map[1]);
                //top of 1
                output += (map[1] != 0) ? "──────" : "      ";
                //upper connector between 1 and 2
                output += upperConnector((int)map[1], (int)map[2]);
                // rest of 2
                output += (map[2] != 0) ? "──────┐" : "       ";
                //end of first line
                output += '\n';

                //line 2/7

                //side of 0
                output += (map[0] != 0) ? ((map[0] == Segment.cw) ? "^" : "v") : " ";
                //empty space in 0
                output += "      ";
                //middle connector between 0 and 1
                output += middleConnector((int)map[0], (int)map[1]);
                //empty space in 1
                output += "      ";
                //middle connector between 1 and 2
                output += middleConnector((int)map[1], (int)map[2]);
                //empty space in 2
                output += "      ";
                //end of 2
                output += (map[2] != 0) ? ((map[2] == Segment.cw) ? "v" : "^") : " ";
                //end of second line
                output += '\n';

                //line 3/7

                //start of 0
                output += (map[0] != 0) ? "└" : " ";
                //upper ladder of 0 and 3
                output += upperLadder((int)map[0], (int)map[3]);
                //bottom connector of 0 and 1
                output += bottomConnector((int)map[0], (int)map[1]);
                //upper ladder of 1 and 4
                output += upperLadder((int)map[1], (int)map[4]);
                //bottom connector of 1 and 2
                output += bottomConnector((int)map[1], (int)map[2]);
                //upper ladder of 2 and 5
                output += upperLadder((int)map[2], (int)map[5]);
                //end of 2
                output += (map[2] != 0) ? "┘" : " ";
                //end of third line
                output += '\n';

                //0        1         2
                //12345678901234567890123456
                //┌──────┐ ┌──────┐ ┌──────┐
                //│      │ │      │ │      │
                //└\    /┘ └──────┘ └──────┘
                //   ><
                //┌/    \─────────\ /──────┐
                //│                X       ^
                //└───────────────/ \──────┘

                //line 4/7

                //always a space first
                output += ' ';
                //middle ladder of 0 and 3
                output += middleLadder((int)map[0], (int)map[3]);
                //always 3 spaces
                output += "   ";
                //middle ladder of 1 and 4
                output += middleLadder((int)map[1], (int)map[4]);
                //always 3 spaces
                output += "   ";
                //middle ladder of 2 and 5
                output += middleLadder((int)map[2], (int)map[5]);
                //always a space last
                output += ' ';
                //end of fourth line
                output += '\n';

                //line 5/7

                //start of 3
                output += (map[3] != 0) ? "┌" : " ";
                //bottom ladder of 0 and 3
                output += bottomLadder((int)map[0], (int)map[3]);
                //upper connector of 3 and 4
                output += upperConnector((int)map[3], (int)map[4]);
                //bottom ladder of 1 and 4
                output += bottomLadder((int)map[1], (int)map[4]);
                //upper connector of 4 and 5
                output += upperConnector((int)map[4], (int)map[5]);
                //bottom ladder of 2 and 5
                output += bottomLadder((int)map[2], (int)map[5]);
                //end of 5
                output += (map[5] != 0) ? "┐" : " ";
                //end of the fifth line
                output += '\n';

                //line 6/7

                //side of 3
                output += (map[3] != 0) ? ((map[3] == Segment.cw) ? "^" : "v") : " ";
                //empty space in 3
                output += "      ";
                //middle connector between 3 and 4
                output += middleConnector((int)map[3], (int)map[4]);
                //empty space in 4
                output += "      ";
                //middle connector between 4 and 5
                output += middleConnector((int)map[4], (int)map[5]);
                //empty space in 5
                output += "      ";
                //end of 5
                output += (map[5] != 0) ? ((map[5] == Segment.cw) ? "v" : "^") : " ";
                //end of sixth line
                output += '\n';

                //line 7/7

                //start of 3
                output += (map[3] != 0) ? "└──────" : "       ";
                //bottom connector between 3 and 4
                output += bottomConnector((int)map[3], (int)map[4]);
                //bottom of 4
                output += (map[4] != 0) ? "──────" : "      ";
                //bottom connector between 4 and 5
                output += bottomConnector((int)map[4], (int)map[5]);
                //rest of 5
                output += (map[5] != 0) ? "──────┘" : "       ";
                //end of seventh line
                output += '\n';

                //now to see if this worked at all
                Console.Write('\n' + output);

            }
        }

        private string upperConnector(int left, int right)
        {
            string toReturn = "";
            if(left == 0 && right == 0)
            {
                toReturn = "   ";
            }
            else if (left == 0 && right != 0)
            {
                toReturn = "  ┌";
            }
            else if (left != 0 && right == 0)
            {
                toReturn = "┐  ";
            }
            else if (left != 0 && left == right)
            {
                toReturn = "───";
            }
            else if (left != right)
            {
                toReturn = "\\ /";
            }
            return toReturn;
        }

        private string middleConnector(int left, int right)
        {
            string toReturn = "";
            if (left == 0 && right == 0)
            {
                toReturn = "   ";
            }
            else if (left == 0 && right != 0)
            {
                toReturn = "  │";
            }
            else if (left != 0 && right == 0)
            {
                toReturn = "│  ";
            }
            else if (left != 0 && left == right)
            {
                toReturn = "   ";
            }
            else if (left != right)
            {
                toReturn = " X ";
            }
            return toReturn;
        }

        private string bottomConnector(int left, int right)
        {
            string toReturn = "";
            if (left == 0 && right == 0)
            {
                toReturn = "   ";
            }
            else if (left == 0 && right != 0)
            {
                toReturn = "  └";
            }
            else if (left != 0 && right == 0)
            {
                toReturn = "┘  ";
            }
            else if (left != 0 && left == right)
            {
                toReturn = "───";
            }
            else if (left != right)
            {
                toReturn = "/ \\";
            }
            return toReturn;
        }

        private string upperLadder(int top, int bottom)
        {
            string toReturn = "";
            if (top == 0 && bottom == 0)
            {
                toReturn = "      ";
            }
            else if (top == 0 && bottom != 0)
            {
                toReturn = "      ";
            }
            else if (top != 0 && bottom == 0)
            {
                toReturn = "──────";
            }
            else if (top != 0 && top == bottom)
            {
                toReturn = "│    │";
            }
            else if (top != bottom)
            {
                toReturn = "\\    /";
            }
            return toReturn;
        }

        private string middleLadder(int top, int bottom)
        {
            string toReturn = "";
            if (top == 0 && bottom == 0)
            {
                toReturn = "      ";
            }
            else if (top == 0 && bottom != 0)
            {
                toReturn = "      ";
            }
            else if (top != 0 && bottom == 0)
            {
                toReturn = "      ";
            }
            else if (top != 0 && top == bottom)
            {
                toReturn = "│    │";
            }
            else if (top != bottom)
            {
                toReturn = "  ><  ";
            }
            return toReturn;
        }

        private string bottomLadder(int top, int bottom)
        {
            string toReturn = "";
            if (top == 0 && bottom == 0)
            {
                toReturn = "      ";
            }
            else if (top == 0 && bottom != 0)
            {
                toReturn = "──────";
            }
            else if (top != 0 && bottom == 0)
            {
                toReturn = "      ";
            }
            else if (top != 0 && top == bottom)
            {
                toReturn = "│    │";
            }
            else if (top != bottom)
            {
                toReturn = "/    \\";
            }
            return toReturn;
        }
    }
}
