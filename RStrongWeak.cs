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
    /// Show the Relative Strength and Weakness of A Stock Universe
    /// </summary>
    [Description("Show the Relative Strength and Weakness of A Stock Universe")]
    public class RStrongWeak : Indicator
    {
        #region Variables
        // Wizard generated variables
        // User defined variables (add any user defined variables below)
		
		double openingPriceStock;
		double currentPriceStock;
		
		double openingPriceIndex;
		double currentPriceIndex;
		
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
			CalculateOnBarClose = false;
            Add(new Plot(Color.FromKnownColor(KnownColor.Orange), PlotStyle.Line, "Plot0"));
            Overlay				= false;
			Add("SPY", PeriodType.Day, 1);
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
			
			if (CurrentBar <1) return;
			
			// get the openinng price of a stock from the daily
			openingPriceStock = Opens[0][0];
			//Print(openingPriceStock);
			
			currentPriceStock = Closes[0][0];
			//Print(currentPriceStock);
			
			openingPriceIndex = Opens[1][0];
			//Print(openingPriceIndex);
			
			currentPriceIndex = Closes[1][0];
			//Print(currentPriceIndex);

            Plot0.Set( (currentPriceStock/openingPriceStock)   / (currentPriceIndex/openingPriceIndex) );
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Plot0
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
        private RStrongWeak[] cacheRStrongWeak = null;

        private static RStrongWeak checkRStrongWeak = new RStrongWeak();

        /// <summary>
        /// Show the Relative Strength and Weakness of A Stock Universe
        /// </summary>
        /// <returns></returns>
        public RStrongWeak RStrongWeak()
        {
            return RStrongWeak(Input);
        }

        /// <summary>
        /// Show the Relative Strength and Weakness of A Stock Universe
        /// </summary>
        /// <returns></returns>
        public RStrongWeak RStrongWeak(Data.IDataSeries input)
        {
            if (cacheRStrongWeak != null)
                for (int idx = 0; idx < cacheRStrongWeak.Length; idx++)
                    if (cacheRStrongWeak[idx].EqualsInput(input))
                        return cacheRStrongWeak[idx];

            lock (checkRStrongWeak)
            {
                if (cacheRStrongWeak != null)
                    for (int idx = 0; idx < cacheRStrongWeak.Length; idx++)
                        if (cacheRStrongWeak[idx].EqualsInput(input))
                            return cacheRStrongWeak[idx];

                RStrongWeak indicator = new RStrongWeak();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                RStrongWeak[] tmp = new RStrongWeak[cacheRStrongWeak == null ? 1 : cacheRStrongWeak.Length + 1];
                if (cacheRStrongWeak != null)
                    cacheRStrongWeak.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheRStrongWeak = tmp;
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
        /// Show the Relative Strength and Weakness of A Stock Universe
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.RStrongWeak RStrongWeak()
        {
            return _indicator.RStrongWeak(Input);
        }

        /// <summary>
        /// Show the Relative Strength and Weakness of A Stock Universe
        /// </summary>
        /// <returns></returns>
        public Indicator.RStrongWeak RStrongWeak(Data.IDataSeries input)
        {
            return _indicator.RStrongWeak(input);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Show the Relative Strength and Weakness of A Stock Universe
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.RStrongWeak RStrongWeak()
        {
            return _indicator.RStrongWeak(Input);
        }

        /// <summary>
        /// Show the Relative Strength and Weakness of A Stock Universe
        /// </summary>
        /// <returns></returns>
        public Indicator.RStrongWeak RStrongWeak(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.RStrongWeak(input);
        }
    }
}
#endregion
