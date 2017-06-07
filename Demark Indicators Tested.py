import numpy as np
import talib
# Put any initialization logic here.  The context object will be passed to
# the other methods in your algorithm.
def initialize(context):
    #set_universe(universe.DollarVolumeUniverse(floor_percentile=98, ceiling_percentile=100))
    context.stocks = [sid(23175),sid(24),sid(43694),sid(22954),sid(26322),sid(114),sid(630),sid(149),sid(168),sid(8572),sid(270),sid(300)
                            ,sid(14328), sid(659),sid(43495),sid(368),sid(27676),sid(16841),sid(23103),sid(448),sid(455),sid(460),sid(698),
                            sid(794),sid(3806),sid(20330),sid(980),sid(11100),sid(22660),sid(1376),sid(13176),
                            sid(26960),sid(1406),sid(1539), sid(44747),sid(2190),sid(12652)
                            ,sid(2602),sid(2427),sid(2470),
                            sid(10594),sid(2564),sid(9540),sid(44989),sid(2587),sid(2618),sid(21382),sid(27543),sid(43512),
                            sid(42950),sid(2765),sid(2853),sid(2854),sid(32902),sid(3136),sid(3212),sid(9736),sid(46631),
                            sid(47208),sid(20088),sid(3460),sid(41047),sid(3496),
                            sid(216),sid(3620),sid(25090),sid(3647),sid(22651),
                            sid(3718),sid(3766),sid(21774),sid(10187),sid(8655),sid(4031),
                            sid(46053),sid(4151),sid(48317),sid(49229),sid(4263),sid(4315),sid(4373),sid(12909),
                            sid(4487),sid(12691),sid(22096),sid(41451),sid(4537),sid(32146),sid(12350)]
    
    context.index = sid(8554)
    # Trading guard to not trade leveraged ETFs
    set_do_not_order_list(security_lists.leveraged_etf_list)
    
    # Make a separate algo for each stock.
    context.algos_1Min =   [Demark(stock) for stock in context.stocks]

    
    # the leverage buffer to stop trading if leverage exceeds
    context.leverage_buffer = 2.5
    # what is the percentage to risk per position 
    context.percent_per_pos = 0.9
    context.leverage1 = 0

#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~     
# Will be called on every trade event for the securities you specify. 
def handle_data(context, data):
    #record(cash = context.portfolio.cash)
    
    context.leverage1 =  context.account.leverage
    record (leverage = context.leverage1)
     
    # Call each individual algorithm's handle_data method.
    for algo in context.algos_1Min:
        algo.handle_data(context,data ,context.algos_1Min.index(algo))
              
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~        
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
class Demark(object):
   
    def __init__(self,stock):
        
        self.stock = stock
        # to hold the wave3 projection targets
        self.wave3_up_proj = 0
        self.wave3_down_proj = 0
        
        # to hold the stop levels
        self.stop_b = 0
        self.stop_s = 0
        
        self.bars_since_last_setup = 0
        # TD Sequential Phases
        self.setup_phase = 'na'
        self.setup_phase2 = 'na'
        self.count_down_started = 'na'
        
        # to be used to determine what was the last countdown finished
        self.count_down_last_finished = 'na'  
        
        # Current and previous counter since last setup 
        self.__S9_pos_Bcd = 0
        self.__S9_pos_Scd = 0
        self.__S9_pos_combo_Scd = 9
        self.__S9_pos_combo_Bcd = 9
       
        # Current & Previous setup ranges with corrosponding Heighs an Lows
        self.__c_setup_range = 0
        self.__p_setup_range = 0
        self.__c_s_h = 0
        self.__c_s_l = 0
        self.__p_s_h = 0
        self.__p_s_l = 0
        
        # Current and Previous TDST levels variables
        self.curr_TDST_sell = 0
        self.curr_TDST_buy = 0
        self.prev_TDST_sell = 0
        self.prev_TDST_buy = 0
        
        # Arrays to hold the high/low prices of the setup's bars
        self.__Ss_Arr = np.zeros(9)
        self.__Bs_Arr = np.zeros(9)
        # Arrays to hold the closes of the setup's bars
        self.Ss_clos_arr= np.zeros(9)
        self.Bs_clos_arr= np.zeros(9)
        
        # To hold the count of the sell/buy setup
        self.scount = 0
        self.bcount = 0
        
        # To hold the count of the sell/buy countdown
        self.bcd_counter = 0
        self.scd_counter = 0
        
        # To hold the 13 bar close
        self.Bcd13_close = 0
        self.Scd13_close = 0
        
        # To hold the high/low of the 8th bar countdown
        self.eighth_high_low = 0
        
        # TDRisk Levels
        self.Td_s_risk_s = 0
        self.Td_s_risk_b = 0
        self.Td_cd_risk_s = 0
        self.Td_cd_risk_b = 0
        
        #~~~ variables for upTrend wave
        self.d_waves_upt = {'wave0':False,'wave1':False, 'wave2':False , 'wave3':False, 'wave4':False, \
                        'wave5':False, 'waveA':False, 'waveB':False, 'waveC':False}
        
        self.d_w_points_upt = {'wave0_low':0, 'wave1_high':0, 'wave2_low':0, 'wave3_high':0, 'wave4_low':0,\
                           'wave5_high':0, 'waveA_low':0, 'waveB_high':0, 'waveC_low':0}
        
        self.d_waveC_lock_upt = False 
        
        #~~~ variables for downTrend wave
        self.d_waves_downt = {'wave0':False,'wave1':False, 'wave2':False , 'wave3':False, 'wave4':False, \
                        'wave5':False, 'waveA':False, 'waveB':False, 'waveC':False}
        
        self.d_w_points_downt = {'wave0_high':0, 'wave1_low':0, 'wave2_high':0, 'wave3_low':0, 'wave4_high':0,\
                           'wave5_low':0, 'waveA_high':0, 'waveB_low':0, 'waveC_high':0}
        
        self.d_waveC_lock_downt = False 
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~     
    
    def handle_data(self,context,data,index):
        
        if get_open_orders(self.stock):
            return
        
        cash = context.portfolio.cash
        price = data[self.stock].price
        
        order_share = context.portfolio.positions[self.stock].amount

        if order_share > 0:

             if price >= self.wave3_up_proj:     
                 order_target(self.stock,0)

                 log.info('{0}: closing long PROFIT at {1}, selling {2} shares'.format(
                     self.stock.symbol, price, order_share
                 ))

             
             elif price <= self.stop_b :
                 order_target(self.stock,0)

                 log.info('{0}: closing long LOSS at {1}, selling {2} shares'.format(
                     self.stock.symbol, price, order_share
                 ))
               
             
             # Trailing Stop
             elif price - self.stop_b > 2.617 :
                 self.stop_b = self.stop_b + 0.65
                 print ('stop changed = ',self.stop_b)
                    

        elif order_share < 0:

            if price <= self.wave3_down_proj:
                order_target(self.stock,0)

                log.info('{0}: covering short PROFIT at {1}, buying {2} shares'.format(
                    self.stock.symbol,price, order_share
                ))

            
            elif price >= self.stop_s:
                order_target(self.stock,0)

                log.info('{0}: covering short LOSS at {1}, buying {2} shares'.format(
                    self.stock.symbol, price, order_share
                ))

            
            # Trailing Stop    
            elif self.stop_s - price > 2.617 :
                self.stop_s = self.stop_s - 0.65
                print ('stop changed = ',self.stop_s)
        
        else:
            
            if context.leverage1 > context.leverage_buffer:
                return
    
        
            prices_minute = history(15, '1d', 'price')[self.stock]
            rsi_minute = talib.RSI(prices_minute,14) 
            
            prices_index = history(15,'1d','price')[context.index]
            rsi_index = talib.RSI(prices_index,14)
            
            self.d_wave_upt(self.stock)
            self.d_wave_downt(self.stock)
            self.td_setup(self.stock)
            self.td_seq_cd(self.stock,'b')
            self.td_seq_cd(self.stock,'s')
            
            prices_dailyMA = history(51,'1d','price')[self.stock]
            MA200 = talib.MA( prices_dailyMA,50)


            # Trading guard to not trade leveraged ETFs
            if self.stock.symbol not in security_lists.leveraged_etf_list:
            
                if (self.d_waves_upt['wave2'] == True and self.d_waves_upt['wave3'] == False\
                or self.d_waves_upt['wave4'] == True and self.d_waves_upt['wave5'] == False)\
                and rsi_minute[-1] > 40\
                and (self.is9_tradable_lite_v2(self.stock,'b') or self.is13_tradable(self.stock,'b')):
               
                    # Determine the projected level, wether wave 2 or wave 4
                    if self.d_waves_upt['wave2'] == True and self.d_waves_upt['wave3'] == False:

                        self.wave3_up_proj = self.td_wave_projection(3,'u')
                       
                    else:
                        self.wave3_up_proj = self.td_wave_projection(5,'u')
                        
                    # determine what is the stop level, based on setup or cd
                    if self.count_down_last_finished == 'na':
                        self.stop_b = self.Td_s_risk_b - 0.618
                    else:
                        self.stop_b = self.Td_cd_risk_b - 0.618
                    

                    # is it 3 to 1, then enter else, return
                    if (self.wave3_up_proj - price) / (price - self.stop_b) > 3:
                    
                        # Use floor division to get a whole number of shares
                        target_shares = cash*context.percent_per_pos // price

                        order_target(self.stock,+target_shares)
                        #order_target(self.stock,-target_shares,style=StopOrder(self.Td_s_risk_b))
                        
                        log.info('{0}: Price at {1}, buying {2} shares.'.format(
                            self.stock.symbol, price, target_shares
                        ))
                        
                        print ('wave5_up_proj=',self.wave3_up_proj)
                        print ('stop = ',self.stop_b)
                        
                    else:
                        return
                

                if (self.d_waves_downt['wave2'] == True and self.d_waves_downt['wave3'] == False\
                or self.d_waves_downt['wave4'] == True and self.d_waves_downt['wave5'] == False)\
                and rsi_minute[-1] < 60\
                and (self.is9_tradable_lite_v2(self.stock,'s') or self.is13_tradable(self.stock,'s')):
               
                    # Determine the projected level, wether wave 2 or wave 4
                    if self.d_waves_downt['wave2'] == True and self.d_waves_downt['wave3'] == False:
                    
                        self.wave3_down_proj = self.td_wave_projection(3,'d')

                    else:
                        self.wave3_down_proj = self.td_wave_projection(5,'d')
 
                    # determine what is the stop level, based on setup or cd
                    if self.count_down_last_finished == 'na':
                        self.stop_s = self.Td_s_risk_s + 0.618
                    else:
                        self.stop_s = self.Td_cd_risk_s + 0.618
                   
                    # is it 3 to 1, then enter else, return
                    if  (price - self.wave3_down_proj) / (self.stop_s - price) > 3:
                    
                        # Use floor division to get a whole number of shares
                        target_shares = cash*context.percent_per_pos // price

                        order_target(self.stock,-target_shares)
                        #order_target(self.stock,+target_shares,style=StopOrder(self.Td_s_risk_s))

                        log.info('{0}: Price at {1}, shorting {2} shares.'.format(
                            self.stock.symbol, price, target_shares
                        ))
                        
                        print ('wave3_down_proj=',self.wave3_down_proj )
                        print ('stop = ',self.stop_s)
                        
                    else:
                        return


    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~      
    def td_setup(self,stock):
        
        if self.bars_since_last_setup == 9:
            
            close_history = history(14,'1d','close_price')[stock]
            low_history = history(9,'1d','low')[stock]
            high_history = history(9,'1d','high')[stock]

            #~~~~~~~~~~ Sell Setup
            scount = 0
            for i in range(-1,-10,-1):   

                if close_history[i] > close_history[i-4]:
                    scount += 1

                # ensures that the first bar of the setup has got a price flip
                if scount == 9 and close_history[i-1] >= close_history[i-5]:
                    scount = 0

            if scount == 9:
                self.__Ss_Arr[:] = high_history[:]
                self.Ss_clos_arr[:]= close_history[5:]
                #if we have a previous Ss and its countdown is not finished then, current setup is Ss2
                if self.setup_phase == 'Ss':
                    self.setup_phase2 = 'Ss2'
                else:
                    self.setup_phase2 = 'na'

                self.setup_phase = 'Ss'
                self.count_down_last_finished = 'na' 

                # save the previous TDST level
                self.prev_TDST_sell = self.curr_TDST_sell
                # get the TDST of the current setup
                self.curr_TDST_sell = min( low_history.min(), close_history[-1:-11:-1].min() ) 

                # set the Td._s_risk_s
                index = high_history.argmax()
                self.Td_s_risk_s = (high_history.max() - low_history[index]) + high_history.max()

                # get the current setup range
                self.__get_c_s_range(low_history,high_history)  
                self.bars_since_last_setup = 0
                return 'Ss'

            #~~~~~~~~~~ Buy Setup 
            bcount = 0
            for i in range(-1,-10,-1):

                if close_history[i] < close_history[i-4]:
                    bcount += 1
                   
                # ensures that the first bar of the setup has got a price flip
                if bcount == 9 and close_history[i-1] <= close_history[i-5]:
                    bcount = 0

            if bcount == 9:
                self.__Bs_Arr[:] = low_history[:]
                self.Bs_clos_arr[:]= close_history[5:]
                #if we have a previous Bs and its countdown is not finished then, current setup is Bs2
                if self.setup_phase == 'Bs':
                    self.setup_phase2 = 'Bs2'
                else:
                    self.setup_phase2 = 'na'

                self.setup_phase = 'Bs'
                self.count_down_last_finished = 'na' 

                # save the previous TDST level
                self.prev_TDST_buy = self.curr_TDST_buy
                # get the TDST of the current setup
                self.curr_TDST_buy = max(high_history.max(), close_history[-1:-11:-1].max())  

                # set the Td_s_risk_b
                index = low_history.argmin()
                self.Td_s_risk_b = low_history.min() - (high_history[index] - low_history.min()) 

                # get the current setup range   
                self.__get_c_s_range(low_history,high_history)  
                self.bars_since_last_setup = 0
                return 'Bs' 
        else:
            self.bars_since_last_setup += 1
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
    def td_seq_cd(self,stock,x):
    
        self.__is_c_cd_canceled(stock)
     
        #~~~~~~~ Sell CountDown
        if x == 's':
            if self.setup_phase == 'Ss':

                self.__S9_pos_Bcd = 0
                self.__S9_pos_Scd += 1
                self.count_down_started = 'Scd'
                
            #~~~~~~~~~~ 
                # save the current setup range to be used for the cancellation qualifiers I,II
                self.__p_s_h = self.__c_s_h
                self.__p_s_l = self.__c_s_l
                self.__p_setup_range = self.__c_setup_range
                
                # if the lookback not yet >= 13 then exit
                if self.__S9_pos_Scd < 13:
                    return

            #~~~~~~~~~~ Stock Series  
                current_close = history(self.__S9_pos_Scd,'1d','close_price')[stock]
                high_2bars_Ago = history(self.__S9_pos_Scd+2,'1d','high')[stock]
                low_history = history(self.__S9_pos_Scd,'1d','low')[stock]

            #~~~~~~~~~~ Sell CountDown Logic
                scd_counter = 0
                for i in range(self.__S9_pos_Scd):

                    if scd_counter == 12:
                        if current_close[i] > high_2bars_Ago[i] and current_close[i] > eighth_high_low:
                            scd_counter += 1
                            
                    elif current_close[i] > high_2bars_Ago[i]:
                        scd_counter += 1
                       
                    if scd_counter == 8:
                        eighth_high_low =high_2bars_Ago[i+2]

                    if scd_counter == 13:
                        self.setup_phase = 'na' 
                        self.count_down_started = 'na' 
                        self.count_down_last_finished = 'Scd'
                        self.Scd13_close = current_close[-1]
                        scd_counter= 0
                        # TD CD Risk Level
                        index = high_2bars_Ago[2:].argmax()
                        self.Td_cd_risk_s = (high_2bars_Ago[2:].max() - low_history[index]) + high_2bars_Ago[2:].max()
                        
                        self.__S9_pos_Scd = 0
                        self.setup_phase2 = 'na'
                        return 'Scd'
                            
        #~~~~~~~ Buy CountDown   
        elif x == 'b' :
            if self.setup_phase == 'Bs' :

                self.__S9_pos_Scd = 0
                self.__S9_pos_Bcd += 1
                self.count_down_started = 'Bcd'
            
                 #~~~~~~~~~~ 
                # save the current setup range to be used for the cancellation qualifiers I,II
                self.__p_s_h = self.__c_s_h
                self.__p_s_l = self.__c_s_l
                self.__p_setup_range = self.__c_setup_range
                
                # if the lookback not yet >= 13 then exit
                if self.__S9_pos_Bcd < 13:
                    return

            #~~~~~~~~~~ Stock Series  
                current_close = history(self.__S9_pos_Bcd,'1d','close_price')[stock]
                low_2bars_Ago = history(self.__S9_pos_Bcd+2,'1d','low')[stock]
                high_history =  history(self.__S9_pos_Bcd,'1d','high')[stock]

            #~~~~~~~~~~ Buy CountDown Logic
                bcd_counter = 0
                for i in range(self.__S9_pos_Bcd):

                    if bcd_counter == 12:
                        if current_close[i] < low_2bars_Ago[i] and current_close[i] < eighth_high_low :

                            bcd_counter += 1

                    elif current_close[i] < low_2bars_Ago[i]:
                        bcd_counter += 1

                    if bcd_counter == 8:
                        eighth_high_low =low_2bars_Ago[i+2]

                    if bcd_counter == 13: 
                        self.setup_phase = 'na'
                        self.count_down_started = 'na' 
                        self.count_down_last_finished = 'Bcd'
                        self.Bcd13_close = current_close[-1]
                        bcd_counter = 0
                        # TD CD Risk Level
                        index = low_2bars_Ago[2:].argmin()
                        self.Td_cd_risk_b = (high_history[index]- low_2bars_Ago[2:].min()) - low_2bars_Ago[2:].min()
                        
                        self.__S9_pos_Bcd = 0
                        self.setup_phase2 = 'na'
                        return 'Bcd' 
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    def td_combo_cd(self,stock,x):
    
        self.__is_c_cd_combo_canceled(stock)
     
        #~~~~~~~ Sell CountDown
        if x == 's':
            if self.setup_phase == 'Ss':

                self.__S9_pos_combo_Bcd = 9
                self.__S9_pos_combo_Scd += 1
                self.count_down_started = 'Scd'
                
            #~~~~~~~~~~ 
                # save the current setup range to be used for the cancellation qualifiers I,II
                self.__p_s_h = self.__c_s_h
                self.__p_s_l = self.__c_s_l
                self.__p_setup_range = self.__c_setup_range
                
                # if the lookback not yet >= 13 then exit
                if self.__S9_pos_combo_Scd < 13:
                    return

            #~~~~~~~~~~ Stock Series  
                close_history = history(self.__S9_pos_combo_Scd+1,'1d','close_price')[stock]
                high_2bars_Ago = history(self.__S9_pos_combo_Scd+2,'1d','high')[stock]
                low_history = history(self.__S9_pos_combo_Scd+1,'1d','low')[stock]

            #~~~~~~~~~~ Sell CountDown Logic
                scd_counter = 0
                prev_combo_cd_close = 0
                for i in range(1,self.__S9_pos_combo_Scd):

                    if scd_counter == 0:
                        if close_history[i] >= high_2bars_Ago[i-1] and high_2bars_Ago[i+1] >= high_2bars_Ago[i]\
                        and close_history[i] >= close_history[i-1]:
                            
                            scd_counter += 1
                            prev_combo_cd_close = close_history[i]

                    else:
                        if close_history[i] >= high_2bars_Ago[i-1] and high_2bars_Ago[i+1] >= high_2bars_Ago[i]\
                        and close_history[i] >= close_history[i-1] and close_history[i] > prev_combo_cd_close:
                        
                            scd_counter += 1
                            prev_combo_cd_close = close_history[i]

                    if scd_counter == 13:
                        self.setup_phase = 'na' 
                        self.count_down_started = 'na' 
                        self.count_down_last_finished = 'Scd'
                        self.Scd13_close = close_history[-1]
                        scd_counter= 0
                        # TD CD Risk Level
                        index = high_2bars_Ago[2:].argmax()
                        self.Td_cd_risk_s = (high_2bars_Ago[2:].max() - low_history[index]) + high_2bars_Ago[2:].max()
                        
                        self.__S9_pos_combo_Scd = 9
                        self.setup_phase2 = 'na'
                        return 'Scd'
                            
        #~~~~~~~ Buy CountDown   
        elif x == 'b' :
            if self.setup_phase == 'Bs' :

                self.__S9_pos_combo_Scd = 9
                self.__S9_pos_combo_Bcd += 1
                self.count_down_started = 'Bcd'
            
                 #~~~~~~~~~~ 
                # save the current setup range to be used for the cancellation qualifiers I,II
                self.__p_s_h = self.__c_s_h
                self.__p_s_l = self.__c_s_l
                self.__p_setup_range = self.__c_setup_range
                
                # if the lookback not yet >= 13 then exit
                if self.__S9_pos_combo_Bcd < 13:
                    return

            #~~~~~~~~~~ Stock Series  
                close_history = history(self.__S9_pos_combo_Bcd+1,'1d','close_price')[stock]
                low_2bars_Ago = history(self.__S9_pos_combo_Bcd+2,'1d','low')[stock]
                high_history =  history(self.__S9_pos_combo_Bcd,'1d','high')[stock]

            #~~~~~~~~~~ Buy CountDown Logic
                bcd_counter = 0
                for i in range(1,self.__S9_pos_Bcd):

                    if bcd_counter == 0:
                        if close_history[i] <= low_2bars_Ago[i-1] and low_2bars_Ago[i+1] <= low_2bars_Ago[i]\
                        and close_history[i] <= close_history[i-1]:
                            
                            bcd_counter += 1
                            prev_combo_cd_close = close_history[i]

                    elif close_history[i] <= low_2bars_Ago[i-1] and low_2bars_Ago[i+1] <= low_2bars_Ago[i]\
                    and close_history[i] <= close_history[i-1] and close_history[i] < prev_combo_cd_close:
                        
                        bcd_counter += 1
                        prev_combo_cd_close = close_history[i]

                    if bcd_counter == 13: 
                        self.setup_phase = 'na'
                        self.count_down_started = 'na' 
                        self.count_down_last_finished = 'Bcd'
                        self.Bcd13_close = close_history[-1]
                        bcd_counter = 0
                        # TD CD Risk Level
                        index = low_2bars_Ago[2:].argmin()
                        self.Td_cd_risk_b = (high_history[index]- low_2bars_Ago[2:].min()) - low_2bars_Ago[2:].min()
                        
                        self.__S9_pos_combo_Bcd = 9
                        self.setup_phase2 = 'na'
                        return 'Bcd'     
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    def is_setup_perf(self,stock,x):
        """
        Check if the latest setup is perfected at the time this method is called
        
         0     1     2     3     4     5     6     7     8  #Array Index
         1     2     3     4     5     6     7     8     9  #Setup bars
        """
        #if setup passed is sell
        if x == 's':
            high_history = history(2,'1d','high')[stock]
            
            if high_history[-1] >= self.__Ss_Arr[5] and high_history[-1] >= self.__Ss_Arr[6]\
            or high_history[-2] >= self.__Ss_Arr[5] and high_history[-2] >= self.__Ss_Arr[6]:
                return True
        
        # elif setup passed is buy
        elif x == 'b':
            low_history = history(2,'1d', 'low')[stock]
        
            if low_history[-1] <= self.__Bs_Arr[5] and low_history[-1] <= self.__Bs_Arr[6]\
            or low_history[-2] <= self.__Bs_Arr[5] and low_history[-2] <= self.__Bs_Arr[6]:
                return True
            
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~      
    def d_wave_upt(self,stock):
        i = -1
        close_history = history(34,'1d','close_price')[stock]
        
        if self.d_waves_upt['wave0'] == False and close_history[-1] < close_history[-2:-21-i:-1].min():
            self.d_waves_upt['wave0'] = True
            self.d_w_points_upt['wave0_low'] = close_history[-1]
        
        # Guard 
        if self.d_waves_upt['wave0'] == True and self.d_waves_upt['wave2'] == False \
        and close_history[-1] < self.d_w_points_upt['wave0_low']:
            self.d_w_points_upt['wave0_low'] = close_history[-1]
        

        if self.d_waves_upt['wave1'] == False and self.d_waves_upt['wave0'] == True \
        and close_history[-1] > close_history[-2:-13-i:-1].max()\
        and close_history[-1] > self.d_w_points_upt['wave0_low']:    # Guard

            self.d_waves_upt['wave1'] = True
            self.d_w_points_upt['wave1_high'] = close_history[-1]

        if self.d_waves_upt['wave2'] == False and self.d_waves_upt['wave1'] == True:

            if close_history[-1] < close_history[-2:-8-i:-1].min():
                self.d_waves_upt['wave2'] = True
                self.d_w_points_upt['wave2_low'] = close_history[-1]

            # elif current close > the high of wave1, then shift wave1 to the right
            elif close_history[-1] > self.d_w_points_upt['wave1_high']:
                self.d_w_points_upt['wave1_high'] = close_history[-1]

        # If current close violates the low of wave0, then cancel
        # all previous waves and start new wave count
        if self.d_waves_upt['wave2'] == True and (close_history[-1] < self.d_w_points_upt['wave0_low']):

            self.d_waves_upt['wave0'] = False
            self.d_waves_upt['wave1'] = False
            self.d_waves_upt['wave2'] = False
            self.d_w_points_upt['wave0_low'] = 0
            self.d_w_points_upt['wave1_high'] = 0
            self.d_w_points_upt['wave2_low'] = 0


        if self.d_waves_upt['wave3']== False and self.d_waves_upt['wave2']== True:
            if close_history[-1] > close_history[-2:-21-i:-1].max()\
            and close_history[-1] > self.d_w_points_upt['wave1_high']:

                self.d_waves_upt['wave3'] = True
                self.d_w_points_upt['wave3_high'] = close_history[-1]
            
            # elif current close < the low of wave2 then shift wave2 to the right
            elif close_history[-1] < self.d_w_points_upt['wave2_low']:
                self.d_w_points_upt['wave2_low'] = close_history[-1]
            
        if self.d_waves_upt['wave4']== False and self.d_waves_upt['wave3']== True :

            if close_history[-1] < close_history[-2:-13-i:-1].min():
                self.d_waves_upt['wave4'] = True
                self.d_w_points_upt['wave4_low'] = close_history[-1]

            # elif current close > the high of wave3, then shift wave3 to the right
            elif close_history[-1] > self.d_w_points_upt['wave3_high']:
                self.d_w_points_upt['wave3_high'] = close_history[-1]

        # if current close violates the low of wave2, then wave2 is shifted to the right
        if self.d_waves_upt['wave4']== True and close_history[-1] < self.d_w_points_upt['wave2_low']:

            self.d_waves_upt['wave3'] = False
            self.d_waves_upt['wave4'] = False
            self.d_w_points_upt['wave3_high'] = 0
            self.d_w_points_upt['wave4_low'] = 0
            
            self.d_w_points_upt['wave2_low'] = close_history[-1]

        if self.d_waves_upt['wave5']== False and self.d_waves_upt['wave4'] == True :
            
            if close_history[-1] > close_history[-2:-34-i:-1].max() \
            and close_history[-1] > self.d_w_points_upt['wave3_high']:
                
                self.d_waves_upt['wave5'] = True
                self.d_w_points_upt['wave5_high'] = close_history[-1]
            
            # elif current close < the low of wave 4, then shift wave4 to the right
            elif close_history[-1] < self.d_w_points_upt['wave4_low']:
                self.d_w_points_upt['wave4_low'] = close_history[-1]

        if self.d_waves_upt['waveA']== False and self.d_waves_upt['wave5']== True:

            if close_history[-1] < close_history[-2:-13-i:-1].min():       
                self.d_waves_upt['waveA'] = True
                self.d_w_points_upt['waveA_low'] = close_history[-1]

            # elif current close > the high of wave5 then shift wave5 to the right
            elif close_history[-1] > self.d_w_points_upt['wave5_high']:
                self.d_w_points_upt['wave5_high'] = close_history[-1]

        if self.d_waves_upt['waveB']== False and self.d_waves_upt['waveA']== True:
            
            if close_history[-1] > close_history[-2:-8-i:-1].max():
                self.d_waves_upt['waveB'] = True
                self.d_w_points_upt['waveB_high'] = close_history[-1]
            
            # elif current close > the high of wave5 then shift wave5 to the right
            elif close_history[-1] > self.d_w_points_upt['wave5_high']:
                self.d_w_points_upt['wave5_high'] = close_history[-1]
                self.d_waves_upt['waveA'] = False
                self.d_w_points_upt['waveA_low'] = 0
            
            # elif current close < the low of waveA, then shift waveA to the right  ##TESTING STILL##
            elif close_history[-1] < self.d_w_points_upt['waveA_low']:
                self.d_w_points_upt['waveA_low'] = close_history[-1]

        if self.d_waves_upt['waveC'] == False and self.d_waves_upt['waveB']== True:
            
            if close_history[-1] < close_history[-2:-21-i:-1].min():
                self.d_waves_upt['waveC']= True
                self.d_w_points_upt['waveC_low'] = close_history[-1]
           
            # elif current close > the high of wave5 then shift wave5 to the right
            elif close_history[-1] > self.d_w_points_upt['wave5_high']:
                self.d_w_points_upt['wave5_high'] = close_history[-1]
                self.d_waves_upt['waveA'] = False
                self.d_waves_upt['waveB'] = False                
                self.d_w_points_upt['waveA_low'] = 0
                self.d_w_points_upt['waveB_high'] = 0
            
            #elif current close > the high of waveB, then shift waveB to the right ##TESTING STILL##
            elif close_history[-1] > self.d_w_points_upt['waveB_high']:
                self.d_w_points_upt['waveB_high'] = close_history[-1]

        if self.d_waveC_lock_upt == False and self.d_waves_upt['waveC']== True:
            
           # if current close < the low of waveA, then lock wave5
           if close_history[-1] < self.d_w_points_upt['waveA_low']:
                self.d_waveC_lock_upt = True
                # set the d_waves values to False, and the d_w_points to 0
                for val in self.d_waves_upt:
                    self.d_waves_upt[val] = False
                for val in self.d_w_points_upt:
                    self.d_w_points_upt[val] = 0
                
                # set the d_waveC_lock to False
                self.d_waveC_lock_upt = False
           
           # If the current close > than the wave 5 high, then shift wave5 to the right
           # and cancel waveA, waveB and waveC
           elif close_history[-1] > self.d_w_points_upt['wave5_high']:
                self.d_w_points_upt['wave5_high'] = close_history[-1]
                self.d_waves_upt['waveA'] = False
                self.d_waves_upt['waveB'] = False
                self.d_waves_upt['waveC'] = False
                self.d_w_points_upt['waveA_low'] = 0
                self.d_w_points_upt['waveB_high'] = 0
                self.d_w_points_upt['waveC_low'] = 0
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    def d_wave_downt(self,stock):
        i = -1
        close_history = history(34,'1d','close_price')[stock]
        
        if self.d_waves_downt['wave0'] == False and close_history[-1] > close_history[-2:-21-i:-1].max():
            self.d_waves_downt['wave0'] = True
            self.d_w_points_downt['wave0_high'] = close_history[-1]
        
        # Guard 
        if self.d_waves_downt['wave0'] == True and self.d_waves_downt['wave2'] == False \
        and close_history[-1] > self.d_w_points_downt['wave0_high']:
            self.d_w_points_downt['wave0_high'] = close_history[-1]
        

        if self.d_waves_downt['wave1'] == False and self.d_waves_downt['wave0'] == True \
        and close_history[-1] < close_history[-2:-13-i:-1].min()\
        and close_history[-1] < self.d_w_points_downt['wave0_high']:    # Guard

            self.d_waves_downt['wave1'] = True
            self.d_w_points_downt['wave1_low'] = close_history[-1]

        if self.d_waves_downt['wave2'] == False and self.d_waves_downt['wave1'] == True:

            if close_history[-1] > close_history[-2:-8-i:-1].max():
                self.d_waves_downt['wave2'] = True
                self.d_w_points_downt['wave2_high'] = close_history[-1]

            # elif current close > the high of wave1, then shift wave1 to the right
            elif close_history[-1] < self.d_w_points_downt['wave1_low']:
                self.d_w_points_downt['wave1_low'] = close_history[-1]

        # If current close violates the low of wave0, then cancel
        # all previous waves and start new wave count
        if self.d_waves_downt['wave2'] == True and (close_history[-1] > self.d_w_points_downt['wave0_high']):

            self.d_waves_downt['wave0'] = False
            self.d_waves_downt['wave1'] = False
            self.d_waves_downt['wave2'] = False
            self.d_w_points_downt['wave0_high'] = 0
            self.d_w_points_downt['wave1_low'] = 0
            self.d_w_points_downt['wave2_high'] = 0


        if self.d_waves_downt['wave3']== False and self.d_waves_downt['wave2']== True:
            if close_history[-1] < close_history[-2:-21-i:-1].min()\
            and close_history[-1] < self.d_w_points_downt['wave1_low']:

                self.d_waves_downt['wave3'] = True
                self.d_w_points_downt['wave3_low'] = close_history[-1]
            
            # elif current close < the low of wave2 then shift wave2 to the right
            elif close_history[-1] > self.d_w_points_downt['wave2_high']:
                self.d_w_points_downt['wave2_high'] = close_history[-1]
            
        if self.d_waves_downt['wave4']== False and self.d_waves_downt['wave3']== True :

            if close_history[-1] > close_history[-2:-13-i:-1].max():
                self.d_waves_downt['wave4'] = True
                self.d_w_points_downt['wave4_high'] = close_history[-1]

            # elif current close > the high of wave3, then shift wave3 to the right
            elif close_history[-1] < self.d_w_points_downt['wave3_low']:
                self.d_w_points_downt['wave3_low'] = close_history[-1]

        # if current close violates the low of wave2, then wave2 is shifted to the right
        if self.d_waves_downt['wave4']== True and close_history[-1] > self.d_w_points_downt['wave2_high']:

            self.d_waves_downt['wave3'] = False
            self.d_waves_downt['wave4'] = False
            self.d_w_points_downt['wave3_low'] = 0
            self.d_w_points_downt['wave4_high'] = 0
            
            self.d_w_points_downt['wave2_high'] = close_history[-1]

        if self.d_waves_downt['wave5']== False and self.d_waves_downt['wave4'] == True :
            
            if close_history[-1] < close_history[-2:-34-i:-1].min() \
            and close_history[-1] < self.d_w_points_downt['wave3_low']:
                
                self.d_waves_downt['wave5'] = True
                self.d_w_points_downt['wave5_low'] = close_history[-1]
            
            # elif current close < the low of wave 4, then shift wave4 to the right
            elif close_history[-1] > self.d_w_points_downt['wave4_high']:
                self.d_w_points_downt['wave4_high'] = close_history[-1]

        if self.d_waves_downt['waveA']== False and self.d_waves_downt['wave5']== True:

            if close_history[-1] > close_history[-2:-13-i:-1].max():       
                self.d_waves_downt['waveA'] = True
                self.d_w_points_downt['waveA_high'] = close_history[-1]

            # elif current close > the high of wave5 then shift wave5 to the right
            elif close_history[-1] < self.d_w_points_downt['wave5_low']:
                self.d_w_points_downt['wave5_low'] = close_history[-1]

        if self.d_waves_downt['waveB']== False and self.d_waves_downt['waveA']== True:
            
            if close_history[-1] < close_history[-2:-8-i:-1].min():
                self.d_waves_downt['waveB'] = True
                self.d_w_points_downt['waveB_low'] = close_history[-1]
            
            # elif current close > the high of wave5 then shift wave5 to the right
            elif close_history[-1] < self.d_w_points_downt['wave5_low']:
                self.d_w_points_downt['wave5_low'] = close_history[-1]
                self.d_waves_downt['waveA'] = False
                self.d_w_points_downt['waveA_high'] = 0
                
            # elif current close > the high of waveA, then shift waveA to the right  ##TESTING STILL##
            elif close_history[-1] > self.d_w_points_downt['waveA_high']:
                self.d_w_points_downt['waveA_high'] = close_history[-1]


        if self.d_waves_downt['waveC'] == False and self.d_waves_downt['waveB']== True:
            
            if close_history[-1] > close_history[-2:-21-i:-1].max():
                self.d_waves_downt['waveC']= True
                self.d_w_points_downt['waveC_high'] = close_history[-1]
           
            # elif current close > the high of wave5 then shift wave5 to the right
            elif close_history[-1] < self.d_w_points_downt['wave5_low']:
                self.d_w_points_downt['wave5_low'] = close_history[-1]
                self.d_waves_downt['waveA'] = False
                self.d_waves_downt['waveB'] = False                
                self.d_w_points_downt['waveA_high'] = 0
                self.d_w_points_downt['waveB_low'] = 0

            #elif current close < the low of waveB, then shift waveB to the right ##TESTING STILL##
            elif close_history[-1] < self.d_w_points_downt['waveB_low']:
                self.d_w_points_downt['waveB_low'] = close_history[-1]     
                
            
        if self.d_waveC_lock_downt == False and self.d_waves_downt['waveC']== True:
            
           # if current close < the low of waveA, then lock wave5
           if close_history[-1] > self.d_w_points_downt['waveA_high']:
                self.d_waveC_lock_downt = True
                # set the d_waves values to False, and the d_w_points to 0
                for val in self.d_waves_downt:
                    self.d_waves_downt[val] = False
                for val in self.d_w_points_downt:
                    self.d_w_points_downt[val] = 0
                
                # set the d_waveC_lock to False
                self.d_waveC_lock_downt = False
           
           # If the current close > than the wave 5 high, then shift wave5 to the right
           # and cancel waveA, waveB and waveC
           elif close_history[-1] < self.d_w_points_downt['wave5_low']:
                self.d_w_points_downt['wave5_low'] = close_history[-1]
                self.d_waves_downt['waveA'] = False
                self.d_waves_downt['waveB'] = False
                self.d_waves_downt['waveC'] = False
                self.d_w_points_downt['waveA_high'] = 0
                self.d_w_points_downt['waveB_low'] = 0
                self.d_w_points_downt['waveC_high'] = 0
                
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~   
    def td_wave_projection(self,waveProjCount,x):
        """
        Returns the up projection level for the passed desired wave
        """
        if x == 'u':
            if waveProjCount == 3:
                wave3Proj = self.d_w_points_upt['wave0_low'] + ((self.d_w_points_upt['wave1_high'] - self.d_w_points_upt['wave0_low']) * 1.618 )
                return wave3Proj
            
            elif waveProjCount == 5:
                wave5Proj = self.d_w_points_upt['wave2_low'] + ((self.d_w_points_upt['wave3_high'] - self.d_w_points_upt['wave2_low']) * 1.618 )
                return wave5Proj
        
        elif x == 'd':
            if waveProjCount == 3:
                wave3Proj = self.d_w_points_downt['wave0_high'] - ((self.d_w_points_downt['wave0_high'] - self.d_w_points_downt['wave1_low']) * 1.618 )
                return wave3Proj
            
            elif waveProjCount == 5:
                wave5Proj = self.d_w_points_downt['wave2_high'] - ((self.d_w_points_downt['wave2_high'] - self.d_w_points_downt['wave3_low']) * 1.618 )
                return wave5Proj
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
    def is9_tradable(self,stock,x):
        """
        Check if the latest setup s/b is perfected and any of its closes is not above/below
        the previous TDST,
        and the max/min close of the recent setup is in close proximity of the prev TDST < 0.25
        and current price is not >/< than the previous TDST,
        and the space between current_price - curr_TDST is bigger by 5 or more than the space
        between the td risk and the current price
        """
        current_price = history(1,'1d','price')[stock]
        
        if x == 's':
        
            if self.setup_phase == 'Ss' and self.is_setup_perf(stock,'s') == True\
            and (self.Ss_clos_arr < self.prev_TDST_buy).all() \
            and self.prev_TDST_buy - self.Ss_clos_arr.max() < 0.25    \
            and current_price[-1] < self.Td_s_risk_s :
            #and current_price[-1] - self.curr_TDST_sell / self.Td_s_risk_s - current_price[-1] >= 5:
            
                return True
            else:
                return False
        
        elif x == 'b':
            
            #if self.setup_phase == 'Bs' \
            if self.setup_phase == 'Bs' and self.is_setup_perf(stock,'b') == True\
            and (self.Bs_clos_arr > self.prev_TDST_sell).all() \
            and self.Bs_clos_arr.max() - self.prev_TDST_sell < 0.25    \
            and current_price[-1] > self.Td_s_risk_b:
            #and self.curr_TDST_buy - current_price[-1] / current_price[-1] - self.Td_s_risk_s >= 5:

                return True
            else:
                return False
    
    
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~         
    def is9_tradable_lite(self,stock,x):
        """
        Check if the latest setup s/b is perfected and any of its closes is not above/below
        the previous TDST,
        and the max/min close of the recent setup is in close proximity of the prev TDST < 0.25
        and current price is not >/< than the previous TDST,
        and the space between current_price - curr_TDST is bigger by 5 or more than the space
        between the td risk and the current price
        """
        current_price = history(1,'1d','price')[stock]
        
        if x == 's':

            if self.setup_phase == 'Ss' and self.is_setup_perf(stock,'s') == True\
            and current_price[-1] < self.Td_s_risk_s :
            #and current_price[-1] - self.curr_TDST_sell / self.Td_s_risk_s - current_price[-1] >= 5:

                return True
            else:
                return False

        elif x == 'b':

            #if self.setup_phase == 'Bs' \
            if self.setup_phase == 'Bs' and self.is_setup_perf(stock,'b') == True\
            and current_price[-1] > self.Td_s_risk_b:
            #and self.curr_TDST_buy - current_price[-1] / current_price[-1] - self.Td_s_risk_s >= 5:

                return True
            else:
                return False
    
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~             
    def is9_tradable_lite_v2(self,stock,x):
        """
        Check if the latest setup s/b is perfected and any of its closes is not above/below
        the previous TDST,
        and the max/min close of the recent setup is in close proximity of the prev TDST < 0.25
        and current price is not >/< than the previous TDST,
        and the space between current_price - curr_TDST is bigger by 5 or more than the space
        between the td risk and the current price
        """
        current_price = history(1,'1d','price')[stock]
        
        if x == 's':

            if self.setup_phase == 'Ss' and current_price[-1] < self.Td_s_risk_s\
            and current_price[-1] > self.curr_TDST_sell:
            #and current_price[-1] - self.curr_TDST_sell / self.Td_s_risk_s - current_price[-1] >= 5:

                return True
            else:
                return False

        elif x == 'b':

            if self.setup_phase == 'Bs'  and current_price[-1] > self.Td_s_risk_b\
            and current_price[-1] < self.curr_TDST_buy:
            #and self.curr_TDST_buy - current_price[-1] / current_price[-1] - self.Td_s_risk_s >= 5:

                return True
            else:
                return False
            
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
    def is13_tradable(self,stock,x):
        """
        Check if we have a 13 bar, Scd or Bcd as disired passed direction 's'/'b'
        and check if its below/above the td risk cd level (is still a valid CD)
        """
        current_price = history(1,'1d','price')[stock]
        
        if x == 's':
            if self.count_down_last_finished == 'Scd' and  current_price[-1] < self.Td_cd_risk_s\
            and (self.Scd13_close - current_price[-1])  / (self.Td_cd_risk_s - self.Scd13_close) < 1.618   :
                return True
            else:
                return False
        
        elif x == 'b':
            if self.count_down_last_finished == 'Bcd' and  current_price[-1] > self.Td_cd_risk_b\
            and (current_price[-1] - self.Bcd13_close) / (self.Bcd13_close - self.Td_cd_risk_b ) < 1.618:
                return True
            else:
                return False
                
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~   
    def __get_c_s_range(self,low_history,high_history):
        """
        Get the current setup range
        """
        self.__c_s_h = high_history.max()
        self.__c_s_l = low_history.min()

        self.__c_setup_range = self.__c_s_h - self.__c_s_l
        
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~       
    def __is_c_cd_canceled(self,stock):
        """" 
        if we have a finished sell/buy setup and a sell/buy count down is already started but not yet finished and countdown 
        cancellation qualifier I or II is active, then reset the count down
        """
    
        if self.count_down_started == 'Scd' and self.setup_phase2 == 'Ss2':
            
            # qualifier I or qualifier II? then cancel the current countdown
            if self.__c_setup_range >= self.__p_setup_range \
            and self.__c_setup_range / self.__p_setup_range <= 1.618 \
            or  self.__c_s_h <=  self.__p_s_h and  self.__c_s_l >= self.__p_s_l: 
                
                self.setup_phase2 = 'na'
                self.count_down_started = 'na'
                self.__S9_pos_Scd = 0
                self.scd_counter = 0
        
        elif self.count_down_started == 'Bcd' and self.setup_phase2 == 'Bs2':
        
            # qualifier I or qualifier II? then cancel the current countdown
            if self.__c_setup_range >= self.__p_setup_range \
            and self.__c_setup_range / self.__p_setup_range <= 1.618 \
            or  self.__c_s_h <=  self.__p_s_h and  self.__c_s_l >= self.__p_s_l: 
                
                self.setup_phase2 = 'na'
                self.count_down_started = 'na'
                self.__S9_pos_Bcd = 0
                self.bcd_counter = 0
                
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~          
    def __is_c_cd_combo_canceled(self,stock):
        """" 
        if we have a finished sell/buy setup and a sell/buy count down is already started but not yet finished and countdown 
        cancellation qualifier I or II is active, then reset the count down
        """
    
        if self.count_down_started == 'Scd' and self.setup_phase2 == 'Ss2':
            
            # qualifier I or qualifier II? then cancel the current countdown
            if self.__c_setup_range >= self.__p_setup_range \
            and self.__c_setup_range / self.__p_setup_range <= 1.618 \
            or  self.__c_s_h <=  self.__p_s_h and  self.__c_s_l >= self.__p_s_l: 
                
                self.setup_phase2 = 'na'
                self.count_down_started = 'na'
                self.__S9_pos_combo_Scd = 0
        
        elif self.count_down_started == 'Bcd' and self.setup_phase2 == 'Bs2':
        
            # qualifier I or qualifier II? then cancel the current countdown
            if self.__c_setup_range >= self.__p_setup_range \
            and self.__c_setup_range / self.__p_setup_range <= 1.618 \
            or  self.__c_s_h <=  self.__p_s_h and  self.__c_s_l >= self.__p_s_l: 
                
                self.setup_phase2 = 'na'
                self.count_down_started = 'na'
                self.__S9_pos_combo_Bcd = 0
                
     
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
