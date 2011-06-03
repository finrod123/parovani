using System;
using System.Diagnostics;

namespace DistributionComputerNS
{
    public class UniformDistributionComputer : DistributionComputer
    {
        public UniformDistributionComputer()
        {}

        public override double NextDouble(double low, double high) {
            return low + (high - low) * uniform.NextDouble();
        }

        public override int Next(int low, int high) {
            return uniform.Next(low, high);
        }
    }

    /*
    public class ExponentialDistributionComputer : DistributionComputer
    {
        readonly double EXP_MAXPROB;
        readonly double EXP_MAXPROBLOG;
        double lambda;

        public ExponentialDistributionComputer(double expMaxProb)
        {
            Debug.Assert(0.0d < expMaxProb && expMaxProb < 1);

            EXP_MAXPROB = expMaxProb;
            EXP_MAXPROBLOG = -Math.Log(1 - EXP_MAXPROB);
        }

        public void setParams(double _low, double _high, double _lambda)
        {
            setParams(_low, _high);
            Debug.Assert(_lambda > 0);

            lambda = _lambda;
        }

        public override double GetValue()
        {
            return ((-Math.Log(1 - uniform.NextDouble()) / lambda) / (EXP_MAXPROBLOG / lambda)) * (high - low) + low;
        }
    }
     * */

    public abstract class DistributionComputer
    {
        protected Random uniform;

        public DistributionComputer()
        {
            uniform = new Random(DateTime.UtcNow.Millisecond);
        }

        public abstract double NextDouble(double low, double high);
        public abstract int Next(int low, int high);
    }
}