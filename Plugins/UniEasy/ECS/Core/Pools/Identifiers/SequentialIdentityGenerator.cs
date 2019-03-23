namespace UniEasy.ECS
{
    public class SequentialIdentityGenerator : IIdentityGenerator
    {
        private int lastIdentifier = 0;

        public int GenerateId()
        {
            return ++lastIdentifier;
        }
    }
}
