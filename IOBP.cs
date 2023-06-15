#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AMIKEIOBP : Indicator
	{
        private double close_;
        private double low_;
        private double high_;
        private double open_;
        private int bar_index_;
		
		private bool highob;
        private bool lowob;
				
		private bool bearob = false;
		private bool bullob = false;
		
		private int     	numberofline;
		private double 		upperphzone;
		private double 		upperplzone;
		private LineData	upperplzoneline;
		private LineData	upperphzoneline;
		private ArrayList	upperphzonearr;
		private ArrayList	upperplzonearr;
		private ArrayList	upperzonetestedarr;

		private double 		lowerphzone;
		private double 		lowerplzone;
		private LineData	lowerplzoneline;
		private LineData	lowerphzoneline;
		private ArrayList	lowerphzonearr;
		private ArrayList	lowerplzonearr;
		private ArrayList	lowerzonetestedarr;

		private struct LineData
		{
			public DateTime startTime { get; set; }
			public double startY { get; set; }
			public DateTime endTime { get; set; }
			public double endY { get; set; }

			public void SetParams(DateTime startTime, double startY, DateTime endTime, double endY) {
				this.startTime = startTime;
				this.startY = startY;
				this.endTime = endTime;
				this.endY = endY;
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Institutional OrderBlock Pressure from TradingView";
				Name										= "AMIKEIOBP";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Showlabels									= true;
				Rline										= true;
				Layout										= @"Wick";
				Highcolor									= Brushes.Red;
				Lowcolor									= Brushes.Green;
				Linestyle									= @"Solid";
				Linesize									= 2;
				Maxlines									= 10;
				Showprice									= true;
				Extend										= false;
				Transp										= 0;
				Ob											= 2;
				Percmove									= 0;
				Obalerts									= true;
				Crossalerts									= false;
				AddPlot(new Stroke(Brushes.Red, 4), PlotStyle.TriangleDown, "BearishOrderBlock");
				AddPlot(new Stroke(Brushes.Green, 4), PlotStyle.TriangleUp, "BullishOrderBlock");
			}
			else if (State == State.Configure)
			{
				numberofline= Maxlines;
				upperphzone = 0;
				upperplzone = 0;
				lowerphzone = 0;
				lowerplzone = 0;

			}
			else if (State == State.DataLoaded)
			{
				upperplzoneline	= new LineData();
				upperphzoneline	= new LineData();
				upperphzonearr = new ArrayList();
				upperplzonearr = new ArrayList();
				upperzonetestedarr = new ArrayList();

				lowerplzoneline	= new LineData();
				lowerphzoneline	= new LineData();
				lowerphzonearr = new ArrayList();
				lowerplzonearr = new ArrayList();
				lowerzonetestedarr = new ArrayList();
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
            int offset = Ob + 1;

			if (CurrentBar <= offset)
				return;
			
			// Print("Count: " + Close.Count + ", CurrentBar: " + CurrentBar);
            close_ = Close[offset];
            low_ = Low[offset];
            high_ = High[offset];
            open_ = Open[offset];
			// bar_index_ = CurrentBar - offset;
			bar_index_ = CurrentBars[0];// - offset;

			int bearcandle 	= 0;
        	int bullcandle 	= 0;
			int po		 	= -offset;
			
			Print("--------------------");
			// Print("bar_index_: " + bar_index_ + ", CurrentBar: " + CurrentBar);
			// Print("high_: " + high_ + ", open_: " + open_ + ", low_: " + low_ + ", close_: " + close_ + ", offset: " + offset);
            
			for (int i = 1; i <= Ob; i++)
            {
				double CloseI = Close[i];
				double OpenI = Open[i];
				// Print("CloseI: " + CloseI + ", OpenI: " + OpenI);

				bearcandle = bearcandle + (CloseI < OpenI ? 1 : 0);
                bullcandle = bullcandle + (CloseI > OpenI ? 1 : 0);
				
				// Print("bearcandle: " + bearcandle + ", bullcandle: " + bullcandle);
            }

            double abs = Math.Abs(Close[offset] - Close[1]) / Close[offset] * 100;
            bool absmove = abs >= Percmove;
            bool beardir = Close[offset] > Open[offset];
            bool bulldir = Close[offset] < Open[offset];
			
			// Print("abs: " + abs);
			// Print("absmove: " + absmove);
			// Print("beardir: " + beardir);
			// Print("bulldir: " + bulldir);
			
            highob = beardir && (bearcandle == (Ob)) && absmove;
            lowob = bulldir && (bullcandle == (Ob)) && absmove;
			
        	double bearobprice = 0.0f;
        	double bullobprice = 0.0f;
			
			bearobprice = highob ? high_ : double.NaN;
			bullobprice = lowob ? low_ : double.NaN;

			bearob = highob;
			bullob = lowob;

		
			// This part is for putting Triangle mark.
			NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 12) { Size = 12, Bold = false };
			
			if (bearob)
			{
			 	BearishOrderBlock[0] = High[0] + 0.1;
				Draw.Text(this, "tagHigh_" + CurrentBar, false, High[0].ToString(), 0, High[0], 50, Brushes.Red, myFont, TextAlignment.Center, null, null, 1);
				CandleOutlineBrushes[0] = Brushes.Green;
			}
			if (bullob)
			{
			 	BullishOrderBlock[0] = Low[0] - 0.1;
				Draw.Text(this, "tagLow_" + CurrentBar, true, Low[0].ToString(), 0, Low[0], -50, Brushes.Green, myFont, TextAlignment.Center, null, null, 1);
				CandleOutlineBrushes[0] = Brushes.Red;
			}
			
			if (bearob && Rline)
			{
				upperphzone = High[0];
				upperplzone = Close[0] < Open[0] ? Close[0] : Open[0];

				if (Layout == "Zone")
				{
					upperplzoneline.SetParams(Bars.GetTime(bar_index_), upperplzone, Bars.GetTime(CurrentBar), upperplzone);
				}

				if (Layout != "Average")
				{
					upperphzoneline.SetParams(Bars.GetTime(bar_index_), upperphzone, Bars.GetTime(CurrentBar), upperphzone);
				}
				else 
				{
					upperphzoneline.SetParams(Bars.GetTime(bar_index_), (upperplzone + upperphzone) / 2, Bars.GetTime(CurrentBar), (upperplzone + upperphzone) / 2);
				}

				if (upperphzonearr.Count > numberofline)
				{
					upperphzonearr.RemoveAt(0);
					upperplzonearr.RemoveAt(0);
					upperzonetestedarr.RemoveAt(0);
				}

				upperphzonearr.Add(upperphzoneline);
				upperplzonearr.Add(upperplzoneline);
				upperzonetestedarr.Add(false);
			}

			ArrayList topRetTextArr = new ArrayList();
			if (upperplzonearr.Count > 0)
			{
				for (int i = 0, j = 0; i < upperplzonearr.Count - 1; i ++)
				{
					LineData tempupperline = (LineData) upperphzonearr[i];
					LineData templowerline = (LineData) upperplzonearr[i];
					bool tested = (bool) upperzonetestedarr[i];
					bool crossed = High[0] > tempupperline.startY;

					if (crossed && !tested)
					{
						upperzonetestedarr[i] = true;
					}
					
					if (Crossalerts && crossed && !tested) 
					{
						upperzonetestedarr[i] = true;
					}
					else if (!tested)
					{
						tempupperline.endTime = Bars.GetTime(CurrentBar);
						upperphzonearr[i] = tempupperline;
						templowerline.endTime = Bars.GetTime(CurrentBar);
						upperplzonearr[i] = templowerline;
						topRetTextArr.Add(tempupperline);
						Print("i: " + i);
						Draw.Text(this, "tagTopRetracement_" + j++, true, "Top Retracement - " + tempupperline.startY, -2, tempupperline.startY, 0, Brushes.Red, myFont, TextAlignment.Left, null, null, 1);
					}

					Draw.Line(this, "upperphzone_" + i, false, tempupperline.startTime, tempupperline.startY, tempupperline.endTime, tempupperline.endY, Brushes.Red, DashStyleHelper.Solid, 2);
					Draw.Line(this, "upperplzone_" + i, false, templowerline.startTime, templowerline.startY, templowerline.endTime, templowerline.endY, Brushes.Red, DashStyleHelper.Solid, 2);
				}
			}


			if (bullob && Rline) 
			{
				lowerphzone = Low[0];
				lowerplzone = Close[0] < Open[0] ? Open[0] : Close[0];

				if (Layout == "Zone")
				{
					lowerplzoneline.SetParams(Bars.GetTime(bar_index_), lowerplzone, Bars.GetTime(CurrentBar), lowerplzone);
				}

				if (Layout != "Average")
				{
					lowerphzoneline.SetParams(Bars.GetTime(bar_index_), lowerphzone, Bars.GetTime(CurrentBar), lowerphzone);
				}
				else 
				{
					lowerphzoneline.SetParams(Bars.GetTime(bar_index_), (lowerplzone + lowerphzone) / 2, Bars.GetTime(CurrentBar), (lowerplzone + lowerphzone) / 2);
				}

				if (lowerphzonearr.Count > numberofline)
				{
					lowerphzonearr.RemoveAt(0);
					lowerplzonearr.RemoveAt(0);
					lowerzonetestedarr.RemoveAt(0);
				}

				lowerphzonearr.Add(lowerphzoneline);
				lowerplzonearr.Add(lowerplzoneline);
				lowerzonetestedarr.Add(false);
			}

			if (lowerplzonearr.Count > 0)
			{
				for (int i = 0, j = 0; i < lowerplzonearr.Count - 1; i ++)
				{
					LineData tempupperline = (LineData) lowerphzonearr[i];
					LineData templowerline = (LineData) lowerplzonearr[i];
					bool tested = (bool) lowerzonetestedarr[i];
					bool crossed = Low[0] < templowerline.startY;

					if (crossed && !tested)
						lowerzonetestedarr[i] = true;
					if (Crossalerts && crossed && !tested)
						lowerzonetestedarr[i] = true;
					else if (!tested)
					{
						tempupperline.endTime = Bars.GetTime(CurrentBar);
						lowerphzonearr[i] = tempupperline;
						templowerline.endTime = Bars.GetTime(CurrentBar);
						lowerplzonearr[i] = templowerline;
						Draw.Text(this, "tagBottomRetracement_" + j++, false, "Bottom Retracement - " + tempupperline.startY, -2, tempupperline.startY, 0, Brushes.Green, myFont, TextAlignment.Left, null, null, 1);
					}

					Draw.Line(this, "lowerphzone_" + i, false, tempupperline.startTime, tempupperline.startY, tempupperline.endTime, tempupperline.endY, Brushes.Green, DashStyleHelper.Solid, 2);
					Draw.Line(this, "lowerplzone_" + i, false, templowerline.startTime, templowerline.startY, templowerline.endTime, templowerline.endY, Brushes.Green, DashStyleHelper.Solid, 2);
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Showlabels", Description="Show OrderBlock Labels", Order=1, GroupName="Parameters")]
		public bool Showlabels
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Rline", Description="Show Retracement Lines", Order=2, GroupName="Parameters")]
		public bool Rline
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Layout", Description="Layout Type", Order=3, GroupName="Parameters")]
		public string Layout
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Highcolor", Description="High Zones Color", Order=4, GroupName="Parameters")]
		public Brush Highcolor
		{ get; set; }

		[Browsable(false)]
		public string HighcolorSerializable
		{
			get { return Serialize.BrushToString(Highcolor); }
			set { Highcolor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Lowcolor", Description="Low Zones Color", Order=5, GroupName="Parameters")]
		public Brush Lowcolor
		{ get; set; }

		[Browsable(false)]
		public string LowcolorSerializable
		{
			get { return Serialize.BrushToString(Lowcolor); }
			set { Lowcolor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Linestyle", Description="Style", Order=6, GroupName="Parameters")]
		public string Linestyle
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Linesize", Description="Size", Order=7, GroupName="Parameters")]
		public int Linesize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Maxlines", Description="Maximum Lines Displayed", Order=8, GroupName="Parameters")]
		public int Maxlines
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Showprice", Description="Show Last Level Price", Order=9, GroupName="Parameters")]
		public bool Showprice
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Extend", Description="Extend Lines", Order=10, GroupName="Parameters")]
		public bool Extend
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Transp", Description="Extended Lines Transparency", Order=11, GroupName="Parameters")]
		public int Transp
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Ob", Description="Offset", Order=12, GroupName="Parameters")]
		public int Ob
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Percmove", Description="Move Percentage", Order=13, GroupName="Parameters")]
		public double Percmove
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Obalerts", Description="Enable OrderBlocks Alerts", Order=14, GroupName="Parameters")]
		public bool Obalerts
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Crossalerts", Description="Enable Top/Bottom Lines Crossed Alerts", Order=15, GroupName="Parameters")]
		public bool Crossalerts
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BearishOrderBlock
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BullishOrderBlock
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AMIKEIOBP[] cacheAMIKEIOBP;
		public AMIKEIOBP AMIKEIOBP(bool showlabels, bool rline, string layout, Brush highcolor, Brush lowcolor, string linestyle, int linesize, int maxlines, bool showprice, bool extend, int transp, int ob, double percmove, bool obalerts, bool crossalerts)
		{
			return AMIKEIOBP(Input, showlabels, rline, layout, highcolor, lowcolor, linestyle, linesize, maxlines, showprice, extend, transp, ob, percmove, obalerts, crossalerts);
		}

		public AMIKEIOBP AMIKEIOBP(ISeries<double> input, bool showlabels, bool rline, string layout, Brush highcolor, Brush lowcolor, string linestyle, int linesize, int maxlines, bool showprice, bool extend, int transp, int ob, double percmove, bool obalerts, bool crossalerts)
		{
			if (cacheAMIKEIOBP != null)
				for (int idx = 0; idx < cacheAMIKEIOBP.Length; idx++)
					if (cacheAMIKEIOBP[idx] != null && cacheAMIKEIOBP[idx].Showlabels == showlabels && cacheAMIKEIOBP[idx].Rline == rline && cacheAMIKEIOBP[idx].Layout == layout && cacheAMIKEIOBP[idx].Highcolor == highcolor && cacheAMIKEIOBP[idx].Lowcolor == lowcolor && cacheAMIKEIOBP[idx].Linestyle == linestyle && cacheAMIKEIOBP[idx].Linesize == linesize && cacheAMIKEIOBP[idx].Maxlines == maxlines && cacheAMIKEIOBP[idx].Showprice == showprice && cacheAMIKEIOBP[idx].Extend == extend && cacheAMIKEIOBP[idx].Transp == transp && cacheAMIKEIOBP[idx].Ob == ob && cacheAMIKEIOBP[idx].Percmove == percmove && cacheAMIKEIOBP[idx].Obalerts == obalerts && cacheAMIKEIOBP[idx].Crossalerts == crossalerts && cacheAMIKEIOBP[idx].EqualsInput(input))
						return cacheAMIKEIOBP[idx];
			return CacheIndicator<AMIKEIOBP>(new AMIKEIOBP(){ Showlabels = showlabels, Rline = rline, Layout = layout, Highcolor = highcolor, Lowcolor = lowcolor, Linestyle = linestyle, Linesize = linesize, Maxlines = maxlines, Showprice = showprice, Extend = extend, Transp = transp, Ob = ob, Percmove = percmove, Obalerts = obalerts, Crossalerts = crossalerts }, input, ref cacheAMIKEIOBP);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AMIKEIOBP AMIKEIOBP(bool showlabels, bool rline, string layout, Brush highcolor, Brush lowcolor, string linestyle, int linesize, int maxlines, bool showprice, bool extend, int transp, int ob, double percmove, bool obalerts, bool crossalerts)
		{
			return indicator.AMIKEIOBP(Input, showlabels, rline, layout, highcolor, lowcolor, linestyle, linesize, maxlines, showprice, extend, transp, ob, percmove, obalerts, crossalerts);
		}

		public Indicators.AMIKEIOBP AMIKEIOBP(ISeries<double> input , bool showlabels, bool rline, string layout, Brush highcolor, Brush lowcolor, string linestyle, int linesize, int maxlines, bool showprice, bool extend, int transp, int ob, double percmove, bool obalerts, bool crossalerts)
		{
			return indicator.AMIKEIOBP(input, showlabels, rline, layout, highcolor, lowcolor, linestyle, linesize, maxlines, showprice, extend, transp, ob, percmove, obalerts, crossalerts);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AMIKEIOBP AMIKEIOBP(bool showlabels, bool rline, string layout, Brush highcolor, Brush lowcolor, string linestyle, int linesize, int maxlines, bool showprice, bool extend, int transp, int ob, double percmove, bool obalerts, bool crossalerts)
		{
			return indicator.AMIKEIOBP(Input, showlabels, rline, layout, highcolor, lowcolor, linestyle, linesize, maxlines, showprice, extend, transp, ob, percmove, obalerts, crossalerts);
		}

		public Indicators.AMIKEIOBP AMIKEIOBP(ISeries<double> input , bool showlabels, bool rline, string layout, Brush highcolor, Brush lowcolor, string linestyle, int linesize, int maxlines, bool showprice, bool extend, int transp, int ob, double percmove, bool obalerts, bool crossalerts)
		{
			return indicator.AMIKEIOBP(input, showlabels, rline, layout, highcolor, lowcolor, linestyle, linesize, maxlines, showprice, extend, transp, ob, percmove, obalerts, crossalerts);
		}
	}
}

#endregion
