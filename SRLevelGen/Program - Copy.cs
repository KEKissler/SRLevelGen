using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRLvlGen
{
    //Originally Authored by: Kyle Kissler - 2/20/2018
    //GitHub: KEKissler
    //
    public class BiasedRNG
    {
        private int divisions;// the number of divisions used to approximate the given function. Give a higher number to get a better representation
        private double totalArea;// total
        private string[] function;// the mathematical function provided after being parsed, just doubles stored as strings
        private Random rng;// default random number generator object

        public BiasedRNG(int numDivisions = 10, string func = "1;0", int seed = 0)//f(x) = 1x^0 is the default func
        {
            rng = new Random(seed);
            divisions = numDivisions;
            setNewFunc(func);
        }

        //checks a given string for validity as a prob dist function and changes this object's function if valid
        public void setNewFunc(string newFunc)
        {
            function = newFunc.Split(';');
            if (function.Length % 2 != 0)
            {
                Console.WriteLine("Given function was invalid, defaulting to f(x) = 1");
                function = new string[] { "1", "0" };
            }
                
            if (!isValidProbDistFunc())
            {
                Console.WriteLine("Given function was recognized as a function, but it is invalid because\nit is negative at at least one point over the interval [0, 1)");
                totalArea = 0.0d;
                function = new string[] { "1", "0" };
                isValidProbDistFunc();
            }
        }

        //this function will make you a simple preset function that biases generation towards a given floating point number between 0 and 1.
        //intended use for this function is a call like: myObj.setNewFunc(BiasedRNG.genFuncBiasedTowards(0.3));
        static public string genFuncBiasedTowards(double target)
        {
            return (6 * Math.Pow(target, 2) - 6 * target - 0.5).ToString() + ";2"
                + (-12 * Math.Pow(target, 3) + 12 * Math.Pow(target, 2) + target).ToString() + ";1"
                + (6 * Math.Pow(target, 4) - 6 * Math.Pow(target, 3) - 0.5 * Math.Pow(target, 2) + 1) + ";0";
        }

        //evaluates to a value on the given function
        private double f(double x)
        {
            double toReturn = 0;
            for (int i = 0; i < function.Length; i += 2)
                toReturn += double.Parse(function[i]) * Math.Pow(x, double.Parse(function[i + 1]));
            return toReturn;
        }

        //evaluates if the function associated with this object is valid
        //also it compiles a value of totalArea when checking the function. so this should be called after assignment of a function
        private bool isValidProbDistFunc()
        {
            // f_x = 0.0d;
            for (double x = 1.0 / (2.0 * divisions); x < 1; x += 1.0 / divisions)
            {
                double f_x = f(x);
                if (f_x < 0)
                    return false;
                totalArea += f_x;
            }
            return true;
        }

        public double nextDouble()
        {
            //this statement centers our currentSelection cursor in the middle of the first possible interval.

            //example: 20 intervals evenly spaced from 0-1 is 0.05 length per division. The first interval starts at 0 and ends at 0.05
            //it is centered at 0.025 or 1/40 or 1/(2*divisons)
            double currentSelection = 1.0 / (2.0 * divisions);
            //the first random number is generated where a random value less than the summation of all interval values
            double intervalSelector = totalArea * rng.NextDouble();
            //subtract the value of the currentSelection interval from that pool of values, intervalSelector,
            //and then move to the next interval and keep subtracting interval sized values from the pool until the pool is exhausted.
            //when this pool is exhausted (becomes 0 or negative) then stop advancing the current selection,
            //it is now the interval whose range will constrain the returned value. 

            //example: 20 intervals, with the function f(x)=2x, totalArea = 9.975, let intervalSelector be something randomly selected,
            //and less than totalArea, say 0.5. The first run of the while loop would subtract the value of the function at that interval
            //from intervalSelector; 0.50 - 2(1/40) = 0.45, the currentSelection would update from 1/40 to 3/40 and the subtraction would repeat
            //for this second interval, because intervalSelector is not yet exhausted.
            //0.45 - 2(3/40) = 0.30 still not exhausted, moving to 3rd interval centered at 5/40
            //0.30 - 2(5/40) = 0.05 still not exhausted, moving to 4th interval centered at 7/40
            //0.05 - 2(7/40) = -0.30 intervalSelector exhausted, not advancing cursor further.
            //the final value of intervalSelector will be 7/40, the 4th interval with a width of 0.05. The returned value will be
            // a randomly selected number in the range 0.35 +/- 0.025.
            while (intervalSelector - f(currentSelection) > 0)
            {
                intervalSelector -= f(currentSelection);
                currentSelection += 1.0 / divisions;
            }
            //length of a range is 1.0 / divisions
            //lowest value in a range is currentSelection - 0.5 / divisions
            //highest value in a range is currentSelection + 0.5 / divisions
            return (currentSelection - 0.5 / divisions) + (1.0 / divisions) * rng.NextDouble();//lowest value in a range + random value within range
        }
    }

    class OldLvlGenerator
    {
        private BiasedRNG b_rng;
        private Random rng;
        private int[,] map = new int[16, 16];
        private int[,,] chanceMap = new int[16, 16, 4];// left, up, right, down chances in that order
        private int limit;
        private Vector2 spawn;

        public OldLvlGenerator(int seed = 0)
        {
            b_rng = new BiasedRNG(10, "1;0;-2;1;1;2", seed);
            rng = new Random(seed);
            resetMap();
            generateChanceMap();
            scaleChanceMap();
            string test = "";
            for (int i = 0; i < 4; ++i)
            {
                for (int x = 0; x < 16; ++x)
                {
                    for (int y = 0; y < 16; ++y)
                    {
                        test += '(';
                            test += chanceMap[x, y, i]<10? chanceMap[x, y, i].ToString() + ' ': chanceMap[x, y, i].ToString();
                        test += ") ";
                    }
                    test += '\n';
                }
                test += '\n';
            }
            Console.WriteLine(test);
        }
        private void generateChanceMap()
        {
            for (int x = 0; x < 16; ++x)
            {
                for (int y = 0; y < 16; ++y)
                {
                    switch (findQuadOf(new Vector2(x, y))) {
                        case 1:
                            chanceMap[x, y, 0] = 70;//main
                            chanceMap[x, y, 1] = (x == 0) ? 0 : (x == y) ? 30 : 10;//out
                            chanceMap[x, y, 2] = 0;//invalid
                            chanceMap[x, y, 3] = (x == y) ? 0 : (x == 0) ? 30 : 20;//in
                            break;
                        case 2:
                            chanceMap[x, y, 0] = 0;//(y == 15) ? 0 : (x + y == 15) ? 14 : 4;//out
                            chanceMap[x, y, 1] = 0;//invalid
                            chanceMap[x, y, 2] = 0;//(x + y == 15) ? 0 : (y == 15) ? 14 : 10;//in
                            chanceMap[x, y, 3] = 100;// 86;//main
                            break;
                        case 3:
                            chanceMap[x, y, 0] = 0;//cases 3 and 4 share unique behavior
                            chanceMap[x, y, 1] = (x==15)?30:20;
                            chanceMap[x, y, 2] = 70;
                            chanceMap[x, y, 3] = (x==15)?0:10;
                            break;
                        case 4:
                            chanceMap[x, y, 0] = 0;
                            chanceMap[x, y, 1] = 0;
                            chanceMap[x, y, 2] = 100;
                            chanceMap[x, y, 3] = 0;
                            break;
                        case 5:
                            break;
                    }
                }
            }
        }
        private void scaleChanceMap()
        {
            for (int x = 0; x < 16; ++x)
            {
                for (int y = 0; y < 16; ++y)
                {
                    int total = 0;
                    for(int i = 0; i < 4; ++i)
                    {
                        total += chanceMap[x, y, i];
                    }
                    if(total != 100 && total != 0)
                    {
                        chanceMap[x, y, 0] = Convert.ToInt32(100.0d/total * chanceMap[x, y, 0]) + 1;
                        chanceMap[x, y, 1] = Convert.ToInt32(100.0d / total * chanceMap[x, y, 1]) + 1;
                        chanceMap[x, y, 2] = Convert.ToInt32(100.0d / total * chanceMap[x, y, 2]) + 1;
                        chanceMap[x, y, 3] = Convert.ToInt32(100.0d / total * chanceMap[x, y, 3]) + 1;
                    }
                }
            }
        }
        private int findQuadOf(Vector2 pos)
        {
            //major axis
            if(pos.x == pos.y)
            {
                return (pos.x <= 7) ? 1 : 3;
            }
            //minor axis
            else if (pos.x == 15-pos.y)
            {
                return (pos.x <= 7) ? 2 : 4;
            }
            //2 or 3
            else if (pos.x + pos.y > 15)
            {
                return (pos.y > pos.x) ? 2 : 3;
            }
            //1 or 4
            else
            {
                return (pos.y > pos.x) ? 1 : 4;
            }
        }
        private void resetMap()
        {
            for (int x = 0; x < 16; ++x)
                for (int y = 0; y < 16; ++y)
                    map[x, y] = 0;
        }
        public struct Vector2
        {
            public Vector2(int newX, int newY)
            {
                x = newX;
                y = newY;
            }
            public int x, y;
        }
        public int[,] generateLevel()
        {
            int[,] toReturn = littleGen();
            Vector2 cursor = new Vector2(spawn.x, spawn.y);
            //int curr = 2;


            return toReturn;
        }
        private int[,] changeAllAdjacent(int[,] toMod, Vector2 pos, int newVal)
        {
            toMod[pos.x + 1, pos.y] = (pos.x == 15) ? newVal : toMod[pos.x + 1, pos.y];
            toMod[pos.x - 1, pos.y] = (pos.x == 0 ) ? newVal : toMod[pos.x - 1, pos.y];
            toMod[pos.x, pos.y + 1] = (pos.y == 15) ? newVal : toMod[pos.x, pos.y + 1];
            toMod[pos.x, pos.y - 1] = (pos.y == 0 ) ? newVal : toMod[pos.x, pos.y - 1];
            return toMod;
        }
        public int[,] littleGen()
        {
            
            resetMap();
            spawn = assignSpawn();
            Vector2 cursor = new Vector2(spawn.x, spawn.y + 1);
            //while not directly below spawn
            Console.Write("limit = " + limit.ToString());
            while (cursor.y != limit || findQuadOf(cursor) != 1)
            {
                //mark where the cursor has been
                map[cursor.x, cursor.y] = 1;
                //move the cursor to the next position
                double selection = 100.0d * rng.NextDouble();
                Console.Write(selection + " ");
                int i = 0;
                for (; i < 4 && selection > 0; ++i)
                {
                    selection -= chanceMap[cursor.x, cursor.y, i];
                }
                switch (i)
                {
                    case 1:
                        cursor.y += 1;
                        break;
                    case 2:
                        cursor.x -= 1;
                        break;
                    case 3:
                        cursor.y -= 1;
                        break;
                    case 4:
                        cursor.x += 1;
                        break;
                    case 5:
                        Console.WriteLine("???");
                        break;
                        
                }
                Console.WriteLine("Changing map[" + cursor.x + ", " + cursor.y + "] to 1");
                if(cursor.x < 0 || cursor.x > 15 || cursor.y < 0 || cursor.y > 15)
                {
                    break;
                }
                map[cursor.x, cursor.y] = 1;
                Console.Write("cursor.y = " + cursor.y.ToString() + ", limit = " + limit.ToString());
            }
            //cleanup
            for (int y = 0; y < 16; ++y)
            {
                int numOnes = 0;
                for (int x = 0; x < 16; ++x)
                {
                    bool start = true;
                    bool keepCounting = false;
                    if (!start && map[x, y] != 1)
                    {
                        keepCounting = true;
                    }
                    if ((start || keepCounting) && map[x, y] == 1)
                    {
                        start = false;
                        ++numOnes;
                    }
                    

                }
                if(numOnes == 1)
                {
                    for (int x = 0; x < 16; ++x)
                    {
                        map[x, y] = 0;
                    }
                }
                else if (numOnes > 1)
                {
                    bool alreadyFound = false;
                    for (int x = 0; x < 16; ++x)
                    {
                        if (alreadyFound && map[x, y] == 1)
                            return map;
                        if (alreadyFound) {
                            map[x, y] = 1;
                        }
                        
                        else if (!alreadyFound && map[x, y] == 1)
                            alreadyFound = true;
                        
                    }
                }
            }
            return map;
        }
        private Vector2 assignSpawn()
        {
            Vector2 toReturn = new Vector2(Convert.ToInt32(8.0 * b_rng.nextDouble()), Convert.ToInt32(8.0 * b_rng.nextDouble()));
            if (toReturn.x > toReturn.y)
            {
                int temp = toReturn.x;
                toReturn.x = toReturn.y;
                toReturn.y = temp;
            }
            map[toReturn.x, toReturn.y] = 1;
            limit = toReturn.y;
            Console.Write("???" + toReturn.x + " " + limit + "    ");
            
            return toReturn;
        }
        
        
    }

    class SRLvlGenerator
    {
        
        public struct Vector2
        {
            public Vector2(int newX, int newY)
            {
                x = newX;
                y = newY;
            }
            public int x, y;
        }
        private BiasedRNG majorGenerator, minorGenerator, nextCursorGenerator;
        private Random rng;
        private List<Vector2> corners;
        private int output = 0;

        public SRLvlGenerator(int seed = 0)
        {
            majorGenerator = new BiasedRNG(10, BiasedRNG.genFuncBiasedTowards(0.5), seed);
            minorGenerator = new BiasedRNG(3, "1;2;-1;1;0.25;0", seed);// odd# divisions with a quadratic function that evaluates to 0 at the center to make size 0 rooms impossible. No 0 minors or majors.
            nextCursorGenerator = new BiasedRNG(10, BiasedRNG.genFuncBiasedTowards(0.5), seed);
            rng = new Random(seed);
        }
        
        
        
        public int[,] generateLevel()
        {
            assignCorners();
            int[,] toReturn = new int[32, 96];//4,1 creates a proper 1 by 4 matrix /shrug
            int maxMajor = 10, minMajor = 5, maxMinor = 5/*5*/, minMinor = -5/*-5*/; //temp location for these critical values
            Vector2 cursor = corners[0];
            for (int i = 0; i < 5; ++i)
            {
                Console.Write("Corner #" + i + " = (" + corners[i].x + ", " + corners[i].y + ")");
                if (i > 0)
                {
                    Vector2 major = getMajorDirection(corners[i - 1], corners[i]);
                    Console.WriteLine("  diff = (" + (corners[i].x - corners[i - 1].x) + ", " + (corners[i].y - corners[i - 1].y) + ")  major = (" + major.x + ", " + major.y + ")");
                }
                else
                {
                    Console.WriteLine();
                }
            }
            for(int corner = 1; corner < 5; ++corner)
            {
                List<Vector2> selectedPoints = new List<Vector2>();


                while (!(cursor.x == corners[corner].x && cursor.y == corners[corner].y))
                {
                    //return new int[,] { { 0 } };
                    //first check if the generation failed and if so, reset cursor and empty the list of selected points and keep generating, also print something error
                    Vector2 diff = new Vector2(corners[corner].x - cursor.x, corners[corner].y - cursor.y);
                    Vector2 major = getMajorDirection(corners[corner - 1], corners[corner]);
                    Vector2 minor = new Vector2(major.y, major.x);
                    //Console.WriteLine("diff: " + str(diff) + ", major: " + str(major) + "Dotted together = " + Dot(diff, major));
                    //Console.WriteLine("\tTargetCorner Major Distance away: " + Dot(diff, major) + "\n\tTargetCorner Minor Distance away: " + Dot(diff,minor));

                    if (Dot(diff, major) <= 0)
                    {
                        Console.WriteLine("Generation between corner #" + (corner - 1) + " and corner #" + (corner) + " failed. Reverting cursor back to corner # " + (corner - 1) + "(" + corners[corner - 1].x + ", " + corners[corner - 1].y + ").");
                        cursor.x = corners[corner - 1].x;
                        cursor.y = corners[corner - 1].y;
                        selectedPoints.Clear();
                    }
                    //OPTIONAL check if the two turn special case needs to execute
                    else if (false)
                    {

                    }
                    //next check if in range for the final jump
                    else if ((minMajor <= Dot(diff, major) && Dot(diff, major) <= maxMajor) && (minMinor <= Dot(diff, minor) && Dot(diff, minor) <= maxMinor))
                    {
                        //here!
                        Console.WriteLine("Jumping to next corner! Updating corner counter, if not yet done.");
                        cursor.x = corners[corner].x;
                        cursor.y = corners[corner].y;
                        selectedPoints.Add(cursor);
                    }
                    // if neither of those were the case, then just generate a point as desired, gen next cursor biased towards intersection point clamped
                    else
                    {
                        //Console.WriteLine("Generating a room normally.");
                        bool slopeIsInfinite = corners[corner].x == corners[corner - 1].x, slopeIsZero = corners[corner].y == corners[corner - 1].y;
                        double slope = slopeIsInfinite ? Double.MaxValue : slopeIsZero ? 0 : ((corners[corner].y - corners[corner - 1].y) / (corners[corner].x - corners[corner - 1].x));
                        //Vector2 diffFromStart = new Vector2(cursor.x - corners[corner - 1].x, cursor.y - corners[corner - 1].y);
                        int majorChoice = (int)(minMajor + (maxMajor - minMajor) * majorGenerator.nextDouble());
                        int minorChoice = (int)(minMinor + (maxMinor - minMinor) * minorGenerator.nextDouble());
                        //Console.WriteLine("majorChoice = " + majorChoice + "   minorChoice = " + minorChoice);
                        Vector2 toAdd = new Vector2(major.x * majorChoice, major.y * majorChoice);//assigns correct direction major
                        toAdd.x = toAdd.x == 0 ? minorChoice : toAdd.x;
                        toAdd.y = toAdd.y == 0 ? minorChoice : toAdd.y;//since minor would be 0 until assigned, previous two lines finds the 0 and assigns accordingly
                        //Console.WriteLine("toAdd: " + str(toAdd));
                        //change this towards bias changes and to support overlapping room stuff
                        Vector2 nextPoint = new Vector2(cursor.x + toAdd.x, cursor.y + toAdd.y);
                        selectedPoints.Add(nextPoint);
                        cursor = nextPoint;
                    }
                }
                foreach (Vector2 v in selectedPoints)
                    //Console.WriteLine(str(v));
                    toReturn[v.y, v.x] = output++;
            }
            foreach (Vector2 c in corners)
                toReturn[c.y, c.x] = 99;
            //int[,] toReturn = new int[,] { {1, 2, 3}, {4, 5, 6} }; // displays like "1 2 3\n4 5 6"
            return toReturn;
        }

        private Vector2 getMajorDirection(Vector2 startPos, Vector2 endPos)
        {
            Vector2 diff = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);
            int pos_i = Dot(diff, new Vector2(1, 0));
            int pos_j = Dot(diff, new Vector2(0, 1));
            int neg_i = Dot(diff, new Vector2(-1, 0));
            int neg_j = Dot(diff, new Vector2(0, -1));
            int max = pos_i;
            max = (pos_j > max) ? pos_j : max;
            max = (neg_i > max) ? neg_i : max;
            max = (neg_j > max) ? neg_j : max;
            return (max == pos_i) ? new Vector2(1, 0) : (max == pos_j) ? new Vector2(0, 1) : (max == neg_i) ? new Vector2(-1, 0) : new Vector2(0, -1);
        }

        private int Dot(Vector2 a, Vector2 b)
        {
            return (a.x * b.x + a.y * b.y);
        }

        private float clamp(float a, float b, float val)
        {
            return (val < a) ? a : (val > b) ? b : val;
        }

        private string str(Vector2 toPrint)
        {
            return "(" + toPrint.x + ", " + toPrint.y + ")";
        }

        public void assignCorners()
        {
            corners = (0.5 >= rng.NextDouble()) ?
                new List<Vector2>() { new Vector2(7, 23), new Vector2(87, 23), new Vector2(87, 7), new Vector2(7, 7), new Vector2(7, 23) }
            :   new List<Vector2>() { new Vector2(7, 23), new Vector2(87, 23), new Vector2(87, 7), new Vector2(7, 15), new Vector2(7, 23) };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string deepLore = "";
            string lastLevelOutput = "";
            Console.Write("Enter a seed to generate: ");
            string input = Console.ReadLine();
            //toggle comment on the following two lines to observe old jank stuff
            //OldLvlGenerator lvlgen = (input.Length > 0) ? new OldLvlGenerator(int.Parse(input)) : new OldLvlGenerator();
            SRLvlGenerator lvlgen = (input.Length > 0) ? new SRLvlGenerator(int.Parse(input)) : new SRLvlGenerator();
            int[,] currentLevel;
            while (!deepLore.Equals("q"))
            {
                lastLevelOutput = "";
                currentLevel = lvlgen.generateLevel();
                for (int x = 0; x < currentLevel.GetLength(0); ++x)
                {
                    for (int y = 0; y < currentLevel.GetLength(1); ++y)
                        lastLevelOutput += currentLevel[x, y].ToString() + ' ';
                    lastLevelOutput += '\n';
                }
                Console.WriteLine(lastLevelOutput);
                Console.WriteLine("Enter q to quit or anything else to generate a level: ");
                deepLore = Console.ReadLine();

            }
        }
    }
}
