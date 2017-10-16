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

        private int _totalViolations;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="context">The context.</param>
        public StylecopHandlers(ICakeContext context)
        {
            _context = context;
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

            Cake.Common.Diagnostics.LoggingAliases.Error(_context, $"{args.Violation.Rule.CheckId}: {args.Message} @ Line {args.LineNumber}");
        }
    }
}