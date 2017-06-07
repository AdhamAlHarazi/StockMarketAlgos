#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Gives the Slope of the Linear Regression
    /// </summary>
    [Description("Gives the Slope of the Linear Regression")]
    public class UpTrendDownTrend : Indicator
    {
        #region Variables
        // Wizard generated variables
            private int period = 10; // Default setting for Period
        // User defined variables (add any user defined variables below)
			private DataSeries seriesOfLinReg;
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Add(new Plot(Color.FromKnownColor(KnownColor.Orange), PlotStyle.Line, "Plot0"));
            Overlay				= false;
			seriesOfLinReg = new DataSeries(this);
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
			
			// get the slope of the linear regression
			seriesOfLinReg.Set(LinRegSlope(9)[0]);
		
			// plot the SMA of the linear regression slope and multiply by 100
            Plot0.Set(((SMA(seriesOfLinReg,20)[0])*100));
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Plot0
        {
            get { return Values[0]; }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int Period
        {
            get { return period; }
            set { period = Math.Max(1, value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private UpTrendDownTrend[] cacheUpTrendDownTrend = null;

        private static UpTrendDownTrend checkUpTrendDownTrend = new UpTrendDownTrend();

        /// <summary>
        /// Gives the Slope of the Linear Regression
        /// </summary>
        /// <returns></returns>
        public UpTrendDownTrend UpTrendDownTrend(int period)
        {
            return UpTrendDownTrend(Input, period);
        }

        /// <summary>
        /// Gives the Slope of the Linear Regression
        /// </summary>
        /// <returns></returns>
        public UpTrendDownTrend UpTrendDownTrend(Data.IDataSeries input, int period)
        {
            if (cacheUpTrendDownTrend != null)
                for (int idx = 0; idx < cacheUpTrendDownTrend.Length; idx++)
                    if (cacheUpTrendDownTrend[idx].Period == period && cacheUpTrendDownTrend[idx].EqualsInput(input))
                        return cacheUpTrendDownTrend[idx];

            lock (checkUpTrendDownTrend)
            {
                checkUpTrendDownTrend.Period = period;
                period = checkUpTrendDownTrend.Period;

                if (cacheUpTrendDownTrend != null)
                    for (int idx = 0; idx < cacheUpTrendDownTrend.Length; idx++)
                        if (cacheUpTrendDownTrend[idx].Period == period && cacheUpTrendDownTrend[idx].EqualsInput(input))
                            return cacheUpTrendDownTrend[idx];

                UpTrendDownTrend indicator = new UpTrendDownTrend();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.Period = period;
                Indicators.Add(indicator);
                indicator.SetUp();

                UpTrendDownTrend[] tmp = new UpTrendDownTrend[cacheUpTrendDownTrend == null ? 1 : cacheUpTrendDownTrend.Length + 1];
                if (cacheUpTrendDownTrend != null)
                    cacheUpTrendDownTrend.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheUpTrendDownTrend = tmp;
                return indicator;
            }
        }
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// Gives the Slope of the Linear Regression
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.UpTrendDownTrend UpTrendDownTrend(int period)
        {
            return _indicator.UpTrendDownTrend(Input, period);
        }

        /// <summary>
        /// Gives the Slope of the Linear Regression
        /// </summary>
        /// <returns></returns>
        public Indicator.UpTrendDownTrend UpTrendDownTrend(Data.IDataSeries input, int period)
        {
            return _indicator.UpTrendDownTrend(input, period);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Gives the Slope of the Linear Regression
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.UpTrendDownTrend UpTrendDownTrend(int period)
        {
            return _indicator.UpTrendDownTrend(Input, period);
        }

        /// <summary>
        /// Gives the Slope of the Linear Regression
        /// </summary>
        /// <returns></returns>
        public Indicator.UpTrendDownTrend UpTrendDownTrend(Data.IDataSeries input, int period)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.UpTrendDownTrend(input, period);
        }
    }
}
#endregion
