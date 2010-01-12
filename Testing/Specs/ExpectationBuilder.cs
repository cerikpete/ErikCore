using Rhino.Mocks;

namespace Testing.Specs
{
    public class ExpectationBuilder<TypeToMock> where TypeToMock : class
    {
        private readonly TypeToMock mock;

        public ExpectationBuilder(TypeToMock mock)
        {
            this.mock = mock;
        }

        public ArgumentBuilder<TypeToMock, ResultOfAction> IsToldTo<ResultOfAction>(Function<TypeToMock, ResultOfAction> action)
        {
            return new ArgumentBuilder<TypeToMock, ResultOfAction>(mock, action);
        }

        public ArgumentBuilder<TypeToMock, ResultOfAction> IsAskedForIts<ResultOfAction>(Function<TypeToMock, ResultOfAction> action)
        {
            return IsToldTo(action);
        }

        public ArgumentBuilder<TypeToMock, ResultOfAction> IsAskedIf<ResultOfAction>(Function<TypeToMock, ResultOfAction> action)
        {
            return IsToldTo(action);
        }
    }
}