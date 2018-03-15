namespace Cake.Stylecop
{
    using Cake.Core;

    using global::StyleCop;
    
    /// <summary>
    /// Stylecop utility class.
    /// </summary>
    public class StylecopHandlers
    {
        private readonly ICakeContext _context;
        private readonly StyleCopSettings _settings;

        private int _totalViolations;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="context">The context.</param>
        public StylecopHandlers(ICakeContext context, StyleCopSettings settings)
        {
            _context = context;
            _settings = settings;
        }

        /// <summary>
        /// The total number of violations.
        /// </summary>
        public int TotalViolations
        {
            get
            {
                return _totalViolations;
            }
        }

        /// <summary>
        /// Called when Stylecop output has been generated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The event args.</param>
        public void OnOutputGenerated(object sender, OutputEventArgs args)
        {
            Cake.Common.Diagnostics.LoggingAliases.Information(_context, args.Output);
        }

        /// <summary>
        /// Called when Stylecop has encountered a rule violation.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The event args.</param>
        public void ViolationEncountered(object sender, ViolationEventArgs args)
        {
            _totalViolations++;

            if (_settings.OutputIssues)
            {
                Cake.Common.Diagnostics.LoggingAliases.Error(
                    _context,
                    string.Format("{0}: {1} @ Line {2}", args.Violation.Rule.CheckId, args.Message, args.LineNumber));
            }
        }
    }
}