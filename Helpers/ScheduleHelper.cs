namespace NexusFlow.Helpers
{
    public static class ScheduleHelper
    {
        public static int CalculateGcd(List<int> numbers)
        {
            return numbers.Aggregate(Gcd);
        }

        public static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

    }
}
