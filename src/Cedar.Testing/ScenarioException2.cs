namespace Cedar.Testing
{
    using System;

    public class ScenarioException2 : Exception
    {
        private readonly ScenarioResult _scenarioResult;

        public ScenarioException2(string message, Exception innerException = null, ScenarioResult scenarioResult = null)
            : base(message, innerException)
        {
            _scenarioResult = scenarioResult;
        }

        public ScenarioResult ScenarioResult
        {
            get { return _scenarioResult; }
        }
    }
}