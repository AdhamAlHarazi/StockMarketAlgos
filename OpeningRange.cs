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
    /// Return Today's range / average range for the last X days
    /// </summary>
    [Description("Return Today's range / average range for the last X days")]
    public class OpeningRange : Indicator
    {
        #region Variables
        // Wizard generated variables
        // User defined variables (add any user defined variables below)
		private DataSeries dvRange;
		double todaysRange;
		double currentReading;
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Add(new Plot(Color.FromKnownColor(KnownColor.Orange), PlotStyle.Line, "Plot0"));
            Overlay				= false;
			BarsRequired = 7800; // Do not plot until the 7800th bar on the chart
			CalculateOnBarClose = true;
			
			dvRange = new DataSeries(this,MaximumBarsLookBack.Infinite);
			
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
            
			
			
				getAvgRangeForLastXDays();
			
				if (CurrentBars[0] < 7800){
				// return 0 readings. so that when calling this indicator from another indicator you can choose the Readings above a specific
				//number safely. (no old <7800 undeeded comparison is disturbing the filteration)
				
					Value.Set(0);
					// exit the indicator
					return;
				}else
					
				
				Value.Set(compareRelativeRange());

        }
	
		// get the range if the time is 94500 for the last X days
		void getAvgRangeForLastXDays(){
			
			dvRange.Set( ToTime(Time[0]) == 094500 ? (MAX(High,15)[0] - MIN(Low,15)[0] ) :0 );
			
		}
		
		// Compare the relative range, today's range / average range for the last X days
		double compareRelativeRange(){
			
			// get today's range if the current time is 094559
			if( ToTime(Time[0]) <= 094559 ){
				todaysRange = (MAX(High,15)[0] - MIN(Low,15)[0] );
			}
			
			// return the ratio
			currentReading = todaysRange / (SUM(dvRange,7800)[0]/20);
			return todaysRange / (SUM(dvRange,7800)[0]/20);
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
        private OpeningRange[] cacheOpeningRange = null;

        private static OpeningRange checkOpeningRange = new OpeningRange();

        /// <summary>
        /// Return Today's range / average range for the last X days
        /// </summary>
        /// <returns></returns>
        public OpeningRange OpeningRange()
        {
            return OpeningRange(Input);
        }

        /// <summary>
        /// Return Today's range / average range for the last X days
        /// </summary>
        /// <returns></returns>
        public OpeningRange OpeningRange(Data.IDataSeries input)
        {
            if (cacheOpeningRange != null)
                for (int idx = 0; idx < cacheOpeningRange.Length; idx++)
                    if (cacheOpeningRange[idx].EqualsInput(input))
                        return cacheOpeningRange[idx];

            lock (checkOpeningRange)
            {
                if (cacheOpeningRange != null)
                    for (int idx = 0; idx < cacheOpeningRange.Length; idx++)
                        if (cacheOpeningRange[idx].EqualsInput(input))
                            return cacheOpeningRange[idx];

                OpeningRange indicator = new OpeningRange();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                OpeningRange[] tmp = new OpeningRange[cacheOpeningRange == null ? 1 : cacheOpeningRange.Length + 1];
                if (cacheOpeningRange != null)
                    cacheOpeningRange.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheOpeningRange = tmp;
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
        /// Return Today's range / average range for the last X days
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.OpeningRange OpeningRange()
        {
            return _indicator.OpeningRange(Input);
        }

        /// <summary>
        /// Return Today's range / average range for the last X days
        /// </summary>
        /// <returns></returns>
        public Indicator.OpeningRange OpeningRange(Data.IDataSeries input)
        {
            return _indicator.OpeningRange(input);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Return Today's range / average range for the last X days
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.OpeningRange OpeningRange()
        {
            return _indicator.OpeningRange(Input);
        }

        /// <summary>
        /// Return Today's range / average range for the last X days
        /// </summary>
        /// <returns></returns>
        public Indicator.OpeningRange OpeningRange(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.OpeningRange(input);
        }
    }
}
#endregion
