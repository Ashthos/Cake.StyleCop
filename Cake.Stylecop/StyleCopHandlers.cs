namespace Cake.Stylecop
{
    using Cake.Core;

    using global::StyleCop;

    using StyleCop;

    public class StylecopHandlers
    {
        private readonly ICakeContext _context;

        private int _totalViolations;

        public StylecopHandlers(ICakeContext context)
        {
            _context = context;
        }

        public int TotalViolations
        {
            get
            {
                return _totalViolations;
            }
        }

        public void OnOutputGenerated(object sender, OutputEventArgs args)
        {
            Cake.Common.Diagnostics.LoggingAliases.Information(_context, args.Output);
        }

        public void ViolationEncountered(object sender, ViolationEventArgs args)
        {
            _totalViolations++;

            Cake.Common.Diagnostics.LoggingAliases.Error(
                _context,
                string.Format("{0}: {1} @ Line {2}", args.Violation.Rule.CheckId, args.Message, args.LineNumber));
        }
    }
}