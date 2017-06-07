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
using System.Threading;
using System.Collections;
#endregion





// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class TDSequential : Indicator
    {
        #region Variables
        // Wizard generated variables
        // User defined variables (add any user defined variables below)
		
		 Font nFont = new Font("Tahoma", 8, FontStyle.Regular);
		 Font ArrowFont = new Font("Stencil", 8, FontStyle.Bold);
		 
		// Buy setup variables
		 int[] bSetupBarsTime = new int[10];
		 int bSetupCounter;
		 int bSC;
		 bool isBuySetup = false;
		
		 double[] buySetupLowsArr = new double[10];
		 int bArrowCounter;
		 bool bSetupArrow=false;
		
		 double lastBuySetupRange;
		 double pBuySetupThigh;
		 double pBuySetupTlow;
		
		
		// Buy countdown variables
		 double[] bCDBarsLows = new double[14];
		 int bCountDownCounter;
		 int bCDC;
		 bool isBuyCountDownStarted = false;
		 int bCDPlusC;
		
		 ArrayList bCountDownBarsLows = new ArrayList();
		 ArrayList bCountDownBarsHighs = new ArrayList();
		 ArrayList bCountDownBarsPCloses = new ArrayList();
		
		
		// Sell setup variables
		 int[] sSetupBarsTime = new int[10];
		 int sSetupCounter;
		 int sSC;
		 bool isSellSetup = false;
	     double[] sellSetupHighsArr = new double[10];
		 int sArrowCounter;
		 bool sSetupArrow = false;
		
		 double lastSellSetupRange;	
		 double pSellSetupThigh;
		 double pSellSetupTlow;
		
		
		// Sell countdown variables
		 double[] sCDBarsHighs = new double[14];
		 int sCountDownCounter;
		 int sCDC;
		 bool isSellCountDownStarted = false;
		 int sCDPlusC;
		
		 ArrayList sCountDownBarsHighs = new ArrayList();
		 ArrayList sCountDownBarsLows = new ArrayList();
		 ArrayList sCountDownBarsPCloses = new ArrayList();
		
		
		//STEALTH Sell CountDown
		double[] SLTHsCDBarsHighs = new double[14];
		int SLTHsCountDownCounter;
		bool isSellSTEALTH = false;
		int STEALTHTagCounter;
		// Variables for both
		 int TDSTCounter;
		 int TDRiskCounter;
	    
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Overlay				= true;
			
			// Calculate on the close of each bar
    		CalculateOnBarClose = true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {

			getNextSetup();
			getNextCountDown();
			
		//	sellCountDownStealth();
			
			
			ifBuySetupPerfectedDrawArrow();
			ifSellSetupPerfectedDrawArrow();

        }
		
		// Countdown function

	    void getNextCountDown(){
			
			//------------------------------ BUY COUNTDOWN ----------------------------------------
			if(isBuySetup == true){
				
				isBuyCountDownStarted = true; // we have a buy countdown started;
				
				if(bCountDownCounter<13 ){ // search for a buy countdown as long as the bars found are less than 13
					
					// save the current bar's Low,High and Previous Close wether it a coundown bar or not, to use it later with TD Risk Level
					bCountDownBarsLows.Add(Low[0]);     
					bCountDownBarsHighs.Add(High[0]);
					bCountDownBarsPCloses.Add(Close[1]);
				
					if(bCountDownCounter == 12){  //do we already have bar 12 and looking for bar 13 now?
						if(Close[0]< Low[2] && Close[0]< bCDBarsLows[8] ){
							bCountDownCounter++;	// counter from 1 to 13
							bCDC++;					// counter for the unique name of the tags
							
							bCDBarsLows[bCountDownCounter] = Low[0];	// we have a countdown evolving get the current bar low
							
							// Draw the numbers based on the buy countdown counter
							DrawText("Bc"+bCDC,true,""+bCountDownCounter, 0,Low[0],-nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
							
						}else if(Close[0] < Low[2] ){
								//bCountDownCounter++;	// dont increment the counter since 13 is not there yet just + sign
								
								bCDPlusC++;   			// counter for the unique name of the stars tags
																
								// Draw the + signs
								DrawText("BcPlus"+bCDPlusC,true,"+", 0,Low[0],-((nFont.Height*2)-(nFont.Height/2)),Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
								
							  }
				    }else{	
						
						if(Close[0] < Low[2])  {
							bCountDownCounter++;	// counter from 1 to 13
							bCDC++;					// counter for the unique name of the tags
					
							bCDBarsLows[bCountDownCounter] =Low[0];
							
							// Draw the numbers based on the buy countdown counter
							//DrawText("Bc"+bCDC,true,""+bCountDownCounter, 0,Low[0],-nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
							DrawText("Bc"+bCDC,true,""+bCountDownCounter, 0,Low[0],-((nFont.Height*2)-(nFont.Height/2)),Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
							
					   }
				   }
			    }		
			    // if we have a finished buy countdown then reset the counter
				if(bCountDownCounter==13){
					bCountDownCounter = 0;	// reset the counter
					isBuySetup = false;  // reset the buysetup status to false to prevent counting countdown unless a new buy setup finishes
					isBuyCountDownStarted = false; // DONT NO IF ITS USEFULL
					bCDPlusC = 0; // reset the buy countdown pluses counter
					// For drawing the countdown risk level
					drawBuyCountDownTDRisk();
					deleteBCTDRiskArrays();
				}  
			}
			
			//----------------------------- SELL COUNTDOWN ------------------------------------------
			if(isSellSetup == true){
				
				isSellCountDownStarted = true; // we have a sell countdown started;
				
				if(sCountDownCounter<13 ){ // search for a sell countdown as long as the bars found are less than 13
					
					// save the current bar's Low,High and Previous Close wether it a coundown bar or not, to use it later with TD Risk Level
					sCountDownBarsLows.Add(Low[0]);     
					sCountDownBarsHighs.Add(High[0]);
					sCountDownBarsPCloses.Add(Close[1]);
					
					if(sCountDownCounter == 12){  //do we already have bar 12 and looking for bar 13 now?
						if(Close[0]> High[2] && Close[0]> sCDBarsHighs[8] ){
							sCountDownCounter++;	// counter from 1 to 13
							sCDC++;					// counter for the unique name of the tags
							
							
							sCDBarsHighs[sCountDownCounter] = High[0];	// we have a countdown evolving get the current bar index based on time
							
							// Draw the numbers based on the buy countdown counter
							DrawText("Sc"+sCDC,true,""+sCountDownCounter, 0,High[0],nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
							
						}else if(Close[0] > High[2] ){
								//bCountDownCounter++;	// dont increment the counter since 13 is not there yet just + sign
								sCDPlusC++;				// counter for the unique name of the stars tags
													
								// Draw the + signs
								DrawText("ScPlus"+sCDPlusC,true,"+", 0,High[0],((nFont.Height*2)-(nFont.Height/2)),Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
							  }
					
				    }else{	
					
						if(Close[0] > High[2])  {
							sCountDownCounter++;	// counter from 1 to 13
							sCDC++;					// counter for the unique name of the tags
							
							sCDBarsHighs[sCountDownCounter] = High[0];	// we have a countdown evolving get the current bar index based on time
							
							// Draw the numbers based on the sell countdown counter
							DrawText("Sc"+sCDC,true,""+sCountDownCounter, 0,High[0],((nFont.Height*2)-(nFont.Height/2)),Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
						}
				   }	
				
			   }	
					
			    // if we have a finished sell countdown then reset the counter
				if(sCountDownCounter==13){
					sCountDownCounter = 0;	// reset the counter
					isSellSetup = false;  // reset the sell setup status to false to prevent counting countdown unless a new buy setup finishes
					isSellCountDownStarted = false; // DONT NO IF ITS USEFULL
					sCDPlusC = 0; // reset the buy countdown pluses counter
					// For drawing the countdown risk level
					drawSellCountDownTDRisk();
					deleteSCTDRiskArrays();
				}  
		    }
			
			
		}
		
	/*------------------------------------------------*/	
		
		// setup function
		
		void getNextSetup(){
			if (CurrentBar < 9) return;
						
			/* ---------- BUY SETUP PHASE ---------------------------------------------- */
			
			// search for a buy setup as long as the bars found are less than 9
			if(bSetupCounter<9 ){
				
				// if we have a setup evolving and the current bar doesnt meet the condtion then reset counter and delete previously drawn texts
				if(bSetupCounter>=1 && (Close[0]< Close[4] != true) ){
					
					// delete unfinished setup
					for(int i=1; i<=bSetupCounter; i++){
						DrawText("B"+(bSC-(i-1)),true, "",bSetupBarsTime[i] ,Low[bSetupBarsTime[i]] , -nFont.Height,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
						//DrawText("B"+(bSC-(i-1)),true, "",0 ,0 , -nFont.Height,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}

					bSetupCounter = 0; //reset counter
				}
				
				// ensures that the first bar of the setup has got a price flip
				if(bSetupCounter==0 &&  Close[0]< Close[4]  && Close[1] > Close[5]){
					
					bSetupCounter++; // counter from 1 to 9
					bSC++;  	     // counter for the unique name of the tags
					buySetupLowsArr[bSetupCounter] = Low[0];   // save the current low into the buy setup array
					bSetupBarsTime[bSetupCounter] = CurrentBar - Bars.GetBar(Time[0]);	// we have a setup evolving get the current bar index based on time

					// Draw the numbers based on the buy setup counter
					DrawText("B"+bSC,true,""+bSetupCounter, 0,Low[0],-nFont.Height,Color.Aqua,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					
				// else if the counter is above 0 then continue with ordinary conditions
				}else{if(bSetupCounter>=1){
				
				
						// check if we have a bar meets the condition to add to the buy setup
						if( Close[0]< Close[4]){
							
							bSetupCounter++; // counter from 1 to 9

							bSC++;  	     // counter for the unique name of the tags
							buySetupLowsArr[bSetupCounter] = Low[0];   // save the current low into the buy setup array
							
							bSetupBarsTime[bSetupCounter] = CurrentBar - Bars.GetBar(Time[0]);	// we have a setup evolving get the current bar index based on time
							
							// Draw the numbers based on the buy setup counter
							DrawText("B"+bSC,true,""+bSetupCounter, 0,Low[0],-nFont.Height,Color.Aqua,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
						}
					 }
				}
				// if we have a finished buy setup and buy count down is already started but not yet finished  and countdown cancellation qualifier I or II is active then reset the count down
				//if(bSetupCounter==9 && isBuyCountDownStarted == true ){
				if( (bSetupCounter==9 && isBuyCountDownStarted == true) 
				&& (((getSetupRange("b") >= lastBuySetupRange) && (getSetupRange("b") / lastBuySetupRange <= 1.618)
					|| getSetupHighLow("h") <= pBuySetupThigh && getSetupHighLow("l") >= pBuySetupTlow)) ){
					
					//delete the previusly drawn buy count down
					for(int i=1; i<=bCountDownCounter; i++){
					
						DrawText("Bc"+(bCDC-(i-1)),true,"",0,bCDBarsLows[i],-nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
					// delete the previously drawn stars
					for(int i=1; i<=bCDPlusC; i++){
						DrawText("BcPlus"+(bCDPlusC-(i-1)),true,"",0,0,-nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
					
				    bCountDownCounter = 0;	// reset the buy countdown counter
					bCDPlusC = 0;		   // reset the buy countdown pluses counter
					
					deleteBCTDRiskArrays(); // delete the buy countdown Td Risk Level Arraylists
				}
			
				
				// if we have a finished buy setup and sell count down is already started but not yet finished then reset the sell count down
				// and delete the previusly drawn sell count down
				if(bSetupCounter==9 && isSellCountDownStarted == true ){
						
				    //delete the previusly drawn count down
					for(int i=1; i<=sCountDownCounter; i++){
						DrawText("Sc"+(sCDC-(i-1)),true,"",0,0,nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
					// delete the previously drawn stars
					for(int i=1; i<=sCDPlusC; i++){
						DrawText("ScPlus"+(sCDPlusC-(i-1)),true,"",0,0,nFont.Height*3,Color.DarkGreen,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
						
				isSellSetup = false;    // this stops the sell countdown from continueing
				isSellCountDownStarted = false;	 // SEEMS USELESS
				sCountDownCounter = 0;	// reset the sell countdown counter
				deleteSCTDRiskArrays(); // delete the buy countdown Td Risk Level Arraylists
					
				}
				
				// if we have a finished buy setup then reset the counter
				if(bSetupCounter==9){
					bSetupCounter = 0;	// reset the counter
					isBuySetup = true;  // turn on th buysetup switch so countdown can begin
					bSetupArrow = false; // set to false to searh for a perfected buy setup(Up Arrow) again starting from this recent finished setup
					drawBuyTDST();      // draw the Buy TDST
					drawBuyTDRisk();	// draw the TD buy Risk level
					lastBuySetupRange = getSetupRange("b"); // save the current setup range into the lastBuySetupRange variable
					//isBearishFlip = false;	
				}  
				

			}
				
				
		/* ----------------------------------------------------------------------------------*/
				
	 	/*---------------SELL SETUP PHASE ---------------------------------------------------*/
				
		// search for a sell setup as long as the bars found are less than 9
			if(sSetupCounter<9 ){
				
				// if we have a setup evolving and the current bar doesnt meet the condtion then reset counter and delete previously drawn texts
				if(sSetupCounter>=1 && (Close[0]> Close[4] != true) ){
	
					// delete unfinished setup
					for(int i=1; i<=sSetupCounter; i++){
						DrawText("S"+(sSC-(i-1)),true, "",sSetupBarsTime[i] ,High[sSetupBarsTime[i]] , +nFont.Height,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
					
				sSetupCounter = 0; //reset counter
					
				}
				
				// ensures that the first bar of the setup has got a price flip
				if(sSetupCounter==0 &&  Close[0]> Close[4]  && Close[1] < Close[5]){
					
						sSetupCounter++; // counter from 1 to 9
						sSC++;  	     // counter for the unique name of the tags
						sellSetupHighsArr[sSetupCounter] = High[0];   // save the current low into the buy setup array
						
						sSetupBarsTime[sSetupCounter] = CurrentBar - Bars.GetBar(Time[0]);	// we have a setup evolving get the current bar index based on time
					

						// Draw the numbers based on the sell setup counter
						DrawText("S"+sSC,true,""+sSetupCounter, 0,High[0],+nFont.Height,Color.Aqua,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					
				// else if the counter is above 0 then continue with ordinary conditions
				}else{ if(sSetupCounter>=1){
				
					// check if we have a bar meets the condition to add to the sell setup
						if( Close[0]> Close[4]){
							
							sSetupCounter++; // counter from 1 to 9
							sSC++;  	     // counter for the unique name of the tags
							sellSetupHighsArr[sSetupCounter] = High[0];   // save the current low into the buy setup array
							
							sSetupBarsTime[sSetupCounter] = CurrentBar - Bars.GetBar(Time[0]);	// we have a setup evolving get the current bar index based on time
							
							// Draw the numbers based on the sell setup counter
							DrawText("S"+sSC,true,""+sSetupCounter, 0,High[0],+nFont.Height,Color.Aqua,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
						}
					  }	
				}	
				// if we have a finished sell setup and a sell count down is already started but not yet finished and countdown cancellation qualifier I or II is active, then reset the count down
				//if(sSetupCounter==9 && isSellCountDownStarted == true ){
			///*	
				if(   (sSetupCounter==9 && isSellCountDownStarted == true) && 
					(((getSetupRange("s") >= lastSellSetupRange) && (getSetupRange("s") / lastSellSetupRange <= 1.618) )
					||(getSetupHighLow("h") <= pSellSetupThigh && getSetupHighLow("l") >= pSellSetupTlow)) ){	
						
					//delete the previously drawn sell count down
					for(int i=1; i<=sCountDownCounter; i++){
						DrawText("Sc"+(sCDC-(i-1)),true,"",0,0,nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}	
					// delete the previously drawn stars
					for(int i=1; i<=sCDPlusC; i++){
						DrawText("ScPlus"+(sCDPlusC-(i-1)),true,"",0,0,nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
					
				    sCountDownCounter = 0;	// reset the sell countdown counter
					sCDPlusC = 0;		// reset the sell countdown pluses counter
					
					// Activate STEALTH Mode
					
					isSellSTEALTH = true;  // activated
					SLTHsCountDownCounter = sCountDownCounter; // copy sCountDownCounter into SLTHsCountDownCounter before resetting it
					//SLTHsCDBarsHighs = sCDBarsHighs.CopyTo(SLTHsCDBarsHighs,14); // NOT SURE
				   // sCDBarsHighs.CopyTo(SLTHsCDBarsHighs,0);// NOT SURE
					copyArrayToArray();
					
					sCountDownCounter = 0;	// reset the sell countdown counter
					deleteSCTDRiskArrays(); // delete the sell countdown Td Risk Level Arraylists
				
					
				}
			//*/
				// if we have a finished sell setup and buy count down is already started but not yet finished then reset the buy count down
				// and delete the previusly drawn buy count down
				if(sSetupCounter==9 && isBuyCountDownStarted == true ){
						
				    //delete the previusly drawn count down
					for(int i=1; i<=bCountDownCounter; i++){
						DrawText("Bc"+(bCDC-(i-1)),true,"",0,0,-nFont.Height*2,Color.DeepPink,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
						// delete the previously drawn stars
					for(int i=1; i<=bCDPlusC; i++){
						DrawText("BcPlus"+(bCDPlusC-(i-1)),true,"",0,0,-nFont.Height*3,Color.DarkGreen,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
					}
						
				isBuySetup = false;    // this stops the sell countdown from continueing
				isBuyCountDownStarted = false;	 // SEEMS USELESS
				bCountDownCounter = 0;	// reset the sell countdown counter
					
				deleteBCTDRiskArrays(); // delete the buy countdown Td Risk Level Arraylists
						
					
				}
		
				// if we have a finished sell setup then reset the counter
				if(sSetupCounter==9){
					sSetupCounter = 0;    // reset the counter
					isSellSetup = true;   // turn on th sellsetup switch so countdown can begin
					sSetupArrow = false;  // set to false to searh for a perfected sell setup(Down Arrow) again starting from this recent finished setup
					drawSellTDST();       // draw the Sell TDST
					drawSellTDRisk();      // Draw sell TD Risk level
					lastSellSetupRange = getSetupRange("s"); // save the current setup range into the lastBuySetupRange variable
				} 
			}
				
			/* ------------------------------------------------------------------------*/
			
		}
		
		// Check if the current buy setup is perfected, if yes then draw an up arrow
		void ifBuySetupPerfectedDrawArrow(){	
			
			if(isBuySetup==true && bSetupArrow == false && ( (Close[0] <= buySetupLowsArr[6] &&  Close[0] <= buySetupLowsArr[7]) || (Close[1] <= buySetupLowsArr[6] &&  Close[1] <= buySetupLowsArr[7]) ) ){
				//IArrowUp arrow = DrawArrowUp(""+bArrowCounter, true, 0, Low[0]- TickSize * 40, Color.DarkViolet);
				DrawText("bArrow"+bArrowCounter,true,"˄",0,Low[0],-ArrowFont.Height/2,Color.Aqua,ArrowFont,StringAlignment.Center,Color.Black,Color.Black,10);
				bArrowCounter++;    // increment the counter for the tags
				bSetupArrow = true; // indicates that an arrow has been drawn so not to search and try to draw another one for the recent setup
				
			}
		}
		
		// Check if the current sell setup is perfected, if yes then draw a down arrow
		void ifSellSetupPerfectedDrawArrow(){	
			
			if(isSellSetup==true && sSetupArrow == false &&  ( (Close[0] >= sellSetupHighsArr[6] &&  Close[0] >= sellSetupHighsArr[7]) || (Close[1] >= buySetupLowsArr[6] &&  Close[1] >= buySetupLowsArr[7]) ) ){
				//IArrowDown  arrow = DrawArrowDown(""+sArrowCounter, true, 0, High[0] +TickSize * 40, Color.DarkViolet);
				DrawText("SArrow"+sArrowCounter,true,"˅",0,High[0],ArrowFont.Height/2,Color.Aqua,ArrowFont,StringAlignment.Center,Color.Black,Color.Black,10);
				sArrowCounter++;    // increment the counter for the tags
				sSetupArrow = true; // indicates that an arrow has been drawn so not to search and try to draw another one for the recent setup
			}
		}
		
		
		// Draw a buy TDST line
			void drawBuyTDST(){
				double trueHigh = High[8];
				
				// loop through the entire BuySetup bars and get the true high
				for(int i=1; i<9; i++){

					if(High[i] > trueHigh ){
						trueHigh = High[i];	
					}
				}
				
				// if the previous close is > than the highest high then it is the true high
				if(trueHigh < Close[9]){
					trueHigh = Close[9];
				}
				
				TDSTCounter++;  // increment the TDST counter
				
				DrawLine("BTDST"+TDSTCounter,true,8,trueHigh,-2,trueHigh,Color.DarkGray,DashStyle.Dash,2);
				
			}
		
			
		// Draw a Sell TDST line	
			void drawSellTDST(){
				double trueLow = Low[8];
				
				// loop through the entire BuySetup bars and get the true low
				for(int i=1; i<9; i++){

					if(Low[i] < trueLow ){
						trueLow = Low[i];	
					}
				}
				
				// if the previous close is < than the lowest low then its the true low
				if(trueLow > Close[9]){
					trueLow = Close[9];
				}
				
				TDSTCounter++;  // increment the TDST counter
				
				DrawLine("STDST"+TDSTCounter,true,8,trueLow,-2,trueLow,Color.DarkGray,DashStyle.Dash,2);
				
				
			}
			
			// get the range of the recent finished setup
			double getSetupRange(String x){
				double trueLow =  Low[8]; 
			    double trueHigh = High[8];
				
				// loop through the entire BuySetup bars and get the true low
				for(int i=1; i<9; i++){

					if(Low[i] < trueLow ){
						trueLow = Low[i];	
					}
				}
				
				// if the previous close is < than the lowest low then its the true low
				if(trueLow > Close[9]){
					trueLow = Close[9];
				}
				
				
				
				// loop through the entire BuySetup bars and get the true high
				for(int i=1; i<9; i++){

					if(High[i] > trueHigh ){
						trueHigh = High[i];	
					}
				}
				
				// if the previous close is > than the highest high then it is the true high
				if(trueHigh < Close[9]){
					trueHigh = Close[9];
				}
				
				// save the true high and true low into the variables based on the type of setup passed (x)
				if(x == "b"){
					pBuySetupThigh = trueHigh ;
					pBuySetupTlow =  trueLow;
				
				}else{
					if(x == "s"){
						pSellSetupThigh = trueHigh ;
						pSellSetupTlow =  trueLow;
					}
				}	
				
				return trueHigh - trueLow;	
				
			}
			
			
			// get the high / low of the recent finished setup
			double  getSetupHighLow(String x){
				double trueLow =  Low[8]; 
			    double trueHigh = High[8];
				
				// loop through the entire BuySetup bars and get the true low
				for(int i=1; i<9; i++){

					if(Low[i] < trueLow ){
						trueLow = Low[i];	
					}
				}
				
				// if the previous close is < than the lowest low then its the true low
				if(trueLow > Close[9]){
					trueLow = Close[9];
				}
				
				
				
				// loop through the entire BuySetup bars and get the true high
				for(int i=1; i<9; i++){

					if(High[i] > trueHigh ){
						trueHigh = High[i];	
					}
				}
				
				// if the previous close is > than the highest high then it is the true high
				if(trueHigh < Close[9]){
					trueHigh = Close[9];
				}
				
				// save the true high and true low into the variables based on the type of setup passed (x)
				if(x == "h"){
					return trueHigh;
				
				}else{
				
					return trueLow;
					
				}	
				
			}
			
			
			// Stealth sell countdown
			void sellCountDownStealth(){
				if(isSellSTEALTH == true){
					
					if(SLTHsCountDownCounter<13 ){ // search for a sell countdown as long as the bars found are less than 13
						
						if(SLTHsCountDownCounter == 12){  //do we already have bar 12 and looking for bar 13 now?
							if(Close[0]> High[2] && Close[0]> SLTHsCDBarsHighs[8] ){
						//	if(Close[0]> High[2]  ){	
								SLTHsCountDownCounter++;	// counter from 1 to 13
						
								SLTHsCDBarsHighs[SLTHsCountDownCounter] = High[0];	// we have a countdown evolving get the current bar index based on time
							}
							
						
						}else{	
						
							if(Close[0] > High[2])  {
								
								SLTHsCountDownCounter++;	// counter from 1 to 13
								SLTHsCDBarsHighs[SLTHsCountDownCounter] = High[0];	// we have a countdown evolving get the current bar index based on time
							}
						}	
					
					}	
						
					// if we have a finished sell countdown then reset the counter
					if(SLTHsCountDownCounter==13){
						STEALTHTagCounter++;
						DrawText("SLTHSc"+STEALTHTagCounter,true,"R", 0,High[0],nFont.Height*2,Color.Black,nFont,StringAlignment.Center,Color.Black,Color.Black,10);
						
						SLTHsCountDownCounter = 0;	// reset the counter
						isSellSTEALTH = false;
						
					}  
				}
			 }
			
			void copyArrayToArray(){
				for(int i=0; i<14; i++){
					SLTHsCDBarsHighs[i] = sCDBarsHighs[i];
				//	Print(sCDBarsHighs[i]);
				//	Print(SLTHsCDBarsHighs[i]);
				}
			}
		
		
		// get the range of the truehigh/truelow of TD Risk Level Calculations
			double getTrueHLrange(String x){
				double trueLow =  Low[0]; 
				double higheOfTrueLow=High[0];
				
				double trueHigh = High[0];
				double lowOfTrueHigh=Low[0];
				
				// loop through the entire BuySetup bars and get the true low
				for(int i=1; i<9; i++){

					if(Low[i] < trueLow ){
						trueLow = Low[i];	
						higheOfTrueLow = High[i];
					}
				}
				
				// if the previous close is < than the lowest low then its the true low
				if(trueLow > Close[1]){
					trueLow = Close[1];
					higheOfTrueLow = High[1];
				}
				
				
				
				// loop through the entire BuySetup bars and get the true high
				for(int i=1; i<9; i++){

					if(High[i] > trueHigh ){
						trueHigh = High[i];	
						lowOfTrueHigh = Low[i];
					}
				}
				
				// if the previous close is > than the highest high then it is the true high
				if(trueHigh < Close[1]){
					trueHigh = Close[1];
					lowOfTrueHigh = Low[1];
				}
				
				// save the true high and true low into the variables based on the type of setup passed (x)
				if(x == "b"){
					return (higheOfTrueLow - trueLow);
				}else
					
				if(x == "s"){
					return (trueHigh - lowOfTrueHigh);
				}else
					return 0;
				
				
			
				
			}
		// Draw a buy TD Risk Level
			void drawBuyTDRisk(){
				double trueLow = Low[0];
				double positinToDrawAt;
				// loop through the entire BuySetup bars and get the true low
				for(int i=1; i<9; i++){

					if(Low[i] < trueLow ){
						trueLow = Low[i];	
					}
				}
				
				// if the previous close is < than the lowest low then its the true low
				if(trueLow > Close[1]){
					trueLow = Close[1];
				}
				
				TDRiskCounter++;  // increment the TDRisk Level counter
				positinToDrawAt = trueLow-getTrueHLrange("b");
				
				DrawLine("BTDRisk"+TDRiskCounter,true,1,positinToDrawAt,0,positinToDrawAt,Color.Aqua,DashStyle.Solid,1);
				
			}
		
			
		// Draw a Sell TD Risk Level	
			void drawSellTDRisk(){
				double trueHigh = High[0];
				double positinToDrawAt;
				// loop through the entire BuySetup bars and get the true high
				for(int i=1; i<9; i++){

					if(High[i] > trueHigh ){
						trueHigh = High[i];	
					}
				}
				
				// if the previous close is > than the highest high then it is the true high
				if(trueHigh < Close[1]){
					trueHigh = Close[1];
				}
				
				TDRiskCounter++;  // increment the TDRisk Level counter
				//positinToDrawAt = trueHigh;
				positinToDrawAt = trueHigh+getTrueHLrange("s");
				
				
				DrawLine("STDRisk"+TDRiskCounter,true,1,positinToDrawAt,0,positinToDrawAt,Color.Aqua,DashStyle.Solid,1);
				
			}
		// Draw the Buy CountDown TD Risk Level	
			void drawBuyCountDownTDRisk(){
				double lowestLow =  Convert.ToDouble(bCountDownBarsLows[1]);
				
				int indexOfLowestLow=1;
				double trueRange;
				
				for(int i=0; i< bCountDownBarsLows.Count; i++){
					if ( Convert.ToDouble(bCountDownBarsLows[i]) < lowestLow ){
						lowestLow = Convert.ToDouble(bCountDownBarsLows[i]);
						indexOfLowestLow = i;
					}
				
				}
				
				double highestHigh = Convert.ToDouble(bCountDownBarsHighs[indexOfLowestLow]);
				double pClose =  Convert.ToDouble(bCountDownBarsPCloses[indexOfLowestLow]);
				
				trueRange = (Math.Max(Math.Max(highestHigh - lowestLow  , pClose - lowestLow)  , highestHigh -pClose )) ;
				
				TDRiskCounter++;  // increment the TDRisk Level counter
				DrawLine("BCTDRisk"+TDRiskCounter,true,1,(lowestLow-trueRange),0,(lowestLow-trueRange),Color.DeepPink,DashStyle.Solid,1);
			
				
			}
			
			// delete the buy countdown td risk arraylists
			void deleteBCTDRiskArrays(){
				bCountDownBarsLows.Clear();
				bCountDownBarsHighs.Clear();
				bCountDownBarsPCloses.Clear();
			}
		
			// Draw the Sell CountDown TD Risk Level	
			void drawSellCountDownTDRisk(){
				double highestHigh =  Convert.ToDouble(sCountDownBarsHighs[1]);
		
				int indexOfHighestHigh=1;
				double trueRange;
				
				for(int i=0; i< sCountDownBarsHighs.Count; i++){
					if ( Convert.ToDouble(sCountDownBarsHighs[i]) > highestHigh ){
						highestHigh = Convert.ToDouble(sCountDownBarsHighs[i]);
						indexOfHighestHigh = i;
					}
				
				}
				
				double lowestLow = Convert.ToDouble(sCountDownBarsLows[indexOfHighestHigh]);
				double pClose =  Convert.ToDouble(sCountDownBarsPCloses[indexOfHighestHigh]);
				
				trueRange = (Math.Max(Math.Max(highestHigh - lowestLow  , pClose - lowestLow)  , highestHigh -pClose )) ;
				
				TDRiskCounter++;  // increment the TDRisk Level counter
				DrawLine("SCTDRisk"+TDRiskCounter,true,1,(highestHigh+trueRange),0,(highestHigh+trueRange),Color.DeepPink,DashStyle.Solid,1);
			
				
			}
			
			// delete the Sell countdown td risk arraylists
			void deleteSCTDRiskArrays(){
				sCountDownBarsLows.Clear();
				sCountDownBarsHighs.Clear();
				sCountDownBarsPCloses.Clear();
			}
        #region Properties

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private TDSequential[] cacheTDSequential = null;

        private static TDSequential checkTDSequential = new TDSequential();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public TDSequential TDSequential()
        {
            return TDSequential(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public TDSequential TDSequential(Data.IDataSeries input)
        {
            if (cacheTDSequential != null)
                for (int idx = 0; idx < cacheTDSequential.Length; idx++)
                    if (cacheTDSequential[idx].EqualsInput(input))
                        return cacheTDSequential[idx];

            lock (checkTDSequential)
            {
                if (cacheTDSequential != null)
                    for (int idx = 0; idx < cacheTDSequential.Length; idx++)
                        if (cacheTDSequential[idx].EqualsInput(input))
                            return cacheTDSequential[idx];

                TDSequential indicator = new TDSequential();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                TDSequential[] tmp = new TDSequential[cacheTDSequential == null ? 1 : cacheTDSequential.Length + 1];
                if (cacheTDSequential != null)
                    cacheTDSequential.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheTDSequential = tmp;
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
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.TDSequential TDSequential()
        {
            return _indicator.TDSequential(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.TDSequential TDSequential(Data.IDataSeries input)
        {
            return _indicator.TDSequential(input);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.TDSequential TDSequential()
        {
            return _indicator.TDSequential(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.TDSequential TDSequential(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.TDSequential(input);
        }
    }
}
#endregion
