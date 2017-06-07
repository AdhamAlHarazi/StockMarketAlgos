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
    /// Spike in price
    /// </summary>
    [Description("Spike in price")]
    public class Spike : Indicator
    {
        #region Variables
        // Wizard generated variables
            private int period = 20; // Default setting for Period
        // User defined variables (add any user defined variables below)
		double pClose;
		double StdevOfTheLast20Days;
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Add(new Plot(Color.FromKnownColor(KnownColor.Orange), PlotStyle.Line, "SpikePlot"));
            Overlay				= false;
			CalculateOnBarClose = false;
				
        }
		


        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
            	if (CurrentBar <20) return;
		
				pClose = Close[1];
				StdevOfTheLast20Days = StdDev(13)[0];
		
			//SpikePlot.Set((  Math.Abs( Math.Abs((MarketData.Ask.Price)-pClose)) / StdevOfTheLast20Days ));
				SpikePlot.Set(((  Math.Abs( Math.Abs(Close[0]-pClose)) / StdevOfTheLast20Days ))*1);
			
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries SpikePlot
        {
            get { return Values[0]; }
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
        private Spike[] cacheSpike = null;

        private static Spike checkSpike = new Spike();

        /// <summary>
        /// Spike in price
        /// </summary>
        /// <returns></returns>
        public Spike Spike()
        {
            return Spike(Input);
        }

        /// <summary>
        /// Spike in price
        /// </summary>
        /// <returns></returns>
        public Spike Spike(Data.IDataSeries input)
        {
            if (cacheSpike != null)
                for (int idx = 0; idx < cacheSpike.Length; idx++)
                    if (cacheSpike[idx].EqualsInput(input))
                        return cacheSpike[idx];

            lock (checkSpike)
            {
                if (cacheSpike != null)
                    for (int idx = 0; idx < cacheSpike.Length; idx++)
                        if (cacheSpike[idx].EqualsInput(input))
                            return cacheSpike[idx];

                Spike indicator = new Spike();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                Spike[] tmp = new Spike[cacheSpike == null ? 1 : cacheSpike.Length + 1];
                if (cacheSpike != null)
                    cacheSpike.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheSpike = tmp;
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
        /// Spike in price
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.Spike Spike()
        {
            return _indicator.Spike(Input);
        }

        /// <summary>
        /// Spike in price
        /// </summary>
        /// <returns></returns>
        public Indicator.Spike Spike(Data.IDataSeries input)
        {
            return _indicator.Spike(input);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Spike in price
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.Spike Spike()
        {
            return _indicator.Spike(Input);
        }

        /// <summary>
        /// Spike in price
        /// </summary>
        /// <returns></returns>
        public Indicator.Spike Spike(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.Spike(input);
        }
    }
}
#endregion
