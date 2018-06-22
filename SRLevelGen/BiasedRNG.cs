using System;
//Originally Authored by: Kyle Kissler - 2/20/2018
//GitHub: KEKissler
//
namespace SRLvlGen
{
    public class BiasedRNG
    {
        private int divisions;// the number of divisions used to approximate the given function. Give a higher number to get a better representation
        private double totalArea;// totalArea of the summation of given prod dist func at regular points
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
}