https://www.tradingview.com/script/LL7EEuVI-Institutional-OrderBlock-Pressure/


// This source code is subject to the terms of the Mozilla Public License 2.0 at https://mozilla.org/MPL/2.0/
// © RickSimpson

//@version=5
indicator('Institutional OrderBlock Pressure', 'IOBP', true, max_bars_back=500, max_lines_count=500, max_labels_count=500)

////////////////////////////
//          IOBP          // - ═════ Original Script ═════ : 'Order Block Finder (Experimental)' By Wugamlo => https://tradingview.com/script/R8g2YHdg-Order-Block-Finder-Experimental/
////////////////////////////

//Inputs

showlabels  = input.bool(defval=true,           title='Show OrderBlock Labels?',                                            group='OrderBlock Settings',          tooltip='Display the price of each detected OrderBlock as Labels. Note that you should not in any way interpret these «Arrows» as signals to buy or sell.')
rline       = input.bool(defval=true,           title='Show Retracement Lines?',                                            group='OrderBlock Settings',          tooltip='Displays lines according to detected OrderBlocks Candles. These lines can indicate a potential retracement area.')
layout      = input.string(defval='Wick',       title='Layout Type',                 options=['Wick', 'Zone', 'Average'],   group='OrderBlock Settings')
highcolor   = input.color(defval=color.red,   title='High Zones Color',                                                   group='OrderBlock Settings',          inline='color')
lowcolor    = input.color(defval=color.green, title='Low Zones Color',                                                    group='OrderBlock Settings',          inline='color')
linestyle   = input.string(defval='Solid',      title='Style',                       options=['Solid', 'Dotted', 'Dashed'], group='OrderBlock Settings',          inline='lines')
linessize   = input.int(defval=2,               title='Size',                        minval=1, maxval=2,                    group='OrderBlock Settings',          inline='lines')
maxlines    = input.int(defval=10,              title='Maximum Lines Displayed',     minval=1, maxval=500,                  group='OrderBlock Settings')
showprice   = input.bool(defval=true,           title='Show Last Levels Price?',                                            group='OrderBlock Settings')
extend      = input.bool(defval=false,          title='Extend Lines?',                                                      group='OrderBlock Settings',          tooltip='Extending lines can be usefull to see zones already retraced while keeping in mind that they can still have an impact on the current trend even if they have been already retraced.')
transp      = input.int(defval=0,               title='Extended Lines Transparency', minval=0, maxval=100,                  group='OrderBlock Settings')
ob          = input.int(defval=2,               title='Offset',                      minval=1,                              group='Advanced OrderBlock Settings', tooltip='Relevant periods to identify and search for the potential OrderBlock.')
percmove    = input.float(defval=0,             title='Move Percentage',             minval=0, step=0.1,                    group='Advanced OrderBlock Settings', tooltip='Required number of subsequent Candles in the same direction to identify an OrderBlock. The value you should use may differ between 2 - 5 depending on the Financial Instrument used. (0 = Disabled).')
obalerts    = input.bool(defval=true,           title='Enable OrderBlocks Alerts?',                                         group='Alerts Setup')
crossalerts = input.bool(defval=false,          title='Enable Top/Bottom Lines Crossed Alerts?',                            group='Alerts Setup')

//Color Variables Declaration

highzonescolor  = color(na)
lowzonescolor   = color(na)
highzonescolor := highcolor
lowzonescolor  := lowcolor

//Color Call Function

fzonecolor(obcolor, _call) =>
    c1 = color.r(obcolor)
    c2 = color.g(obcolor)
    c3 = color.b(obcolor)
    color.rgb(c1, c2, c3, _call)

//Round Function

round_f(x) =>
    math.round(x / syminfo.mintick) * syminfo.mintick

//Price Variables Declaration

int offset = ob + 1
close_     = close[offset]
low_       = low[offset]
high_      = high[offset]
open_      = open[offset]
bar_index_ = bar_index[offset]

//OrderBlock Variables Declaration

int bearcandle = 0
int bullcandle = 0
int po         = -offset

//Bearish/Bullish Candle Detection

for i = 1 to ob by 1
    bearcandle := bearcandle  + (close[i] < open[i] ? 1 : 0)
    bearcandle
for i = 1 to ob by 1
    bullcandle := bullcandle  + (close[i] > open[i] ? 1 : 0)
    bullcandle

//OrderBlock Calculation

abs     = math.abs(close[offset] - close[1]) / close[offset] * 100
absmove = abs >= percmove
beardir = close[offset] > open[offset]
bulldir = close[offset] < open[offset]

//OrderBlock Conditions

highob = beardir and (bearcandle == (ob)) and absmove
lowob  = bulldir and (bullcandle == (ob)) and absmove

//Boolean Conditions to Float

var float bearobprice = na
if highob
    bearobprice := high_
    bearobprice
var float bullobprice = na
if lowob
    bullobprice := low_
    bullobprice

//Price Labels Conditions

if showlabels
    l = ta.change(bearobprice) ? label.new(bar_index_, bearobprice[1] + 0.01, str.tostring(round_f(bearobprice)), color=color.new(highzonescolor, 100), textcolor=highzonescolor, style=label.style_label_down, yloc=yloc.abovebar, size=size.small) : ta.change(bullobprice) ? label.new(bar_index_, bullobprice[1] - 0.01, str.tostring(round_f(bullobprice)), color=color.new(lowzonescolor, 100), textcolor=lowzonescolor, style=label.style_label_up, yloc=yloc.belowbar, size=size.small) : na
    l

//Conditions For Retracement Lines

bearob = highob
bullob = lowob

//Bar Color Setup

obc = bearob ? color.new(highzonescolor, 0) : bullob ? color.new(lowzonescolor, 0) : na

//Retracement Lines Variables Declaration

var int     numberofline       = maxlines
var float   upperphzone        = na
var float   upperplzone        = na
var float   lowerphzone        = na
var float   lowerplzone        = na
var line[]  upperphzonearr     = array.new_line(0, na)
var line[]  upperplzonearr     = array.new_line(0, na)
var line[]  lowerphzonearr     = array.new_line(0, na)
var line[]  lowerplzonearr     = array.new_line(0, na)
var line    upperphzoneline    = na
var line    upperplzoneline    = na
var line    lowerphzoneline    = na
var line    lowerplzoneline    = na
var bool[]  upperzonetestedarr = array.new_bool(0, false)
var bool[]  lowerzonetestedarr = array.new_bool(0, false)
var bool    upperzonetested    = false
var bool    lowerzonetested    = false
var bool    nobool             = true
var color   upperzonecolor     = highzonescolor
var color   lowerzonecolor     = lowzonescolor
var label[] labelpharr         = array.new_label(0, na)
var label[] labelplarr         = array.new_label(0, na)
var label   labelph            = na
var label   labelpl            = na

//Extended Retracement Lines Variables Declaration

var box[] bearboxarray       = array.new_box()
var box[] bullboxarray       = array.new_box()
var color bearboxcolor       = color.new(highzonescolor, transp)
var color bullboxcolor       = color.new(lowzonescolor,  transp)
var color bearborderboxcolor = color.new(highzonescolor, transp)
var color bullborderboxcolor = color.new(lowzonescolor,  transp)

//Lines Styles String

f_linestyle(_style) =>
    _style == 'Solid' ? line.style_solid : _style == 'Dotted' ? line.style_dotted : line.style_dashed

//Top Retracement Lines Calculation

if bearob and rline
    upperphzone     := high_
    upperplzone     := close_ < open_             ? close_ : open_
    upperplzoneline := layout == 'Zone'           ? line.new(bar_index_, upperplzone, bar_index, upperplzone, width=linessize) : na
    upperphzoneline := layout != 'Average'        ? line.new(bar_index_, upperphzone, bar_index, upperphzone, width=linessize) : line.new(bar_index_, (upperphzone + upperplzone) / 2, bar_index, (upperphzone + upperplzone) / 2, width=linessize)
    labelph         := showprice                  ? label.new(bar_index, nobool ? upperphzone : (upperphzone + upperplzone) / 2, text=str.tostring(bar_index - bar_index_), textcolor=upperzonecolor, style=label.style_none) : na
    bearbox          = box.new(bar_index_, extend ? upperphzone : na, bar_index, extend ? upperphzone : na, bgcolor=bearboxcolor, border_style=line.style_dotted, border_color=bearborderboxcolor)
    if array.size(upperphzonearr) > numberofline
        line.delete(array.shift(upperphzonearr))
        line.delete(array.shift(upperplzonearr))
        array.shift(upperzonetestedarr)
        label.delete(array.shift(labelpharr))
        box.delete(array.shift(bearboxarray))
    array.push(upperphzonearr, upperphzoneline)
    array.push(upperplzonearr, upperplzoneline)
    array.push(upperzonetestedarr,       false)
    array.push(labelpharr,             labelph)
    array.push(bearboxarray,           bearbox)
if array.size(upperplzonearr) > 0
    for i = 0 to array.size(upperplzonearr) - 1 by 1
        line  tempupperline  = array.get(upperphzonearr,     i)
        line  templowerline  = array.get(upperplzonearr,     i)
        label linepricelabel = array.get(labelpharr,         i)
        bool  tested         = array.get(upperzonetestedarr, i)
        var   boxextend      = array.get(bearboxarray,       i)
        line.set_style(tempupperline, f_linestyle(linestyle))
        line.set_style(templowerline, f_linestyle(linestyle))
        line.set_color(tempupperline, color.from_gradient(i,       1, numberofline, fzonecolor(upperzonecolor, 00), fzonecolor(upperzonecolor, 00)))
        line.set_color(templowerline, color.from_gradient(i,       1, numberofline, fzonecolor(upperzonecolor, 00), fzonecolor(upperzonecolor, 00)))
        label.set_textcolor(linepricelabel, color.from_gradient(i, 1, numberofline, fzonecolor(upperzonecolor, 00), upperzonecolor))
        label.set_text(linepricelabel, str.tostring(round_f(line.get_y1(tempupperline))))
        label.set_text(linepricelabel, '                                             Top Retracement - ' + str.tostring(round_f(line.get_y1(tempupperline))))
        label.set_x(linepricelabel, bar_index)
        box.set_right(array.get(bearboxarray, i), bar_index)
        crossed = high > line.get_y1(tempupperline)
        if crossed and not tested
            array.set(upperzonetestedarr, i, true)
            label.delete(linepricelabel)
        if crossalerts and crossed and not tested
            array.set(upperzonetestedarr, i, true)
            label.delete(linepricelabel)
            alert('Top Line Has Been Crossed! At ' + str.tostring(close), alert.freq_once_per_bar)
        else if not tested
            line.set_x2(tempupperline, bar_index)
            array.set(upperphzonearr, i, tempupperline)
            line.set_x2(templowerline, bar_index)
            array.set(upperplzonearr, i, templowerline)

//Bottom Retracement Lines Calculation

if bullob and rline
    lowerplzone     := low_
    lowerphzone     := close_ < open_             ? open_ : close_
    lowerphzoneline := layout == 'Zone'           ? line.new(bar_index_, lowerphzone, bar_index, lowerphzone, width=linessize) : na
    lowerplzoneline := layout != 'Average'        ? line.new(bar_index_, lowerplzone, bar_index, lowerplzone, width=linessize) : line.new(bar_index_, (lowerphzone + lowerplzone) / 2, bar_index, (lowerphzone + lowerplzone) / 2, width=linessize)
    labelpl         := showprice                  ? label.new(bar_index, nobool ? lowerplzone : (lowerphzone + lowerplzone) / 2, text=str.tostring(bar_index - bar_index_), textcolor=lowerzonecolor, style=label.style_none) : na
    bullbox          = box.new(bar_index_, extend ? lowerplzone : na, bar_index, extend ? lowerplzone : na, bgcolor=bullboxcolor, border_style=line.style_dotted, border_color=bullborderboxcolor)
    if array.size(lowerphzonearr) > numberofline
        line.delete(array.shift(lowerphzonearr))
        line.delete(array.shift(lowerplzonearr))
        array.shift(lowerzonetestedarr)
        label.delete(array.shift(labelplarr))
        box.delete(array.shift(bullboxarray))
    array.push(lowerphzonearr, lowerphzoneline)
    array.push(lowerplzonearr, lowerplzoneline)
    array.push(lowerzonetestedarr,       false)
    array.push(labelplarr,             labelpl)
    array.push(bullboxarray,           bullbox)
if array.size(lowerplzonearr) > 0
    for i = 0 to array.size(lowerplzonearr) - 1 by 1
        line  tempupperline  = array.get(lowerphzonearr,     i)
        line  templowerline  = array.get(lowerplzonearr,     i)
        label linepricelabel = array.get(labelplarr,         i)
        bool  tested         = array.get(lowerzonetestedarr, i)
        var   boxextend      = array.get(bullboxarray,       i)
        line.set_style(tempupperline, f_linestyle(linestyle))
        line.set_style(templowerline, f_linestyle(linestyle))
        line.set_color(tempupperline, color.from_gradient(i,       1, numberofline, fzonecolor(lowerzonecolor, 00), fzonecolor(lowerzonecolor, 00)))
        line.set_color(templowerline, color.from_gradient(i,       1, numberofline, fzonecolor(lowerzonecolor, 00), fzonecolor(lowerzonecolor, 00)))
        label.set_textcolor(linepricelabel, color.from_gradient(i, 1, numberofline, fzonecolor(lowerzonecolor, 00), lowerzonecolor))
        label.set_text(linepricelabel, str.tostring(round_f(line.get_y1(templowerline))))
        label.set_text(linepricelabel, '                                                   Bottom Retracement - ' + str.tostring(round_f(line.get_y1(templowerline))))
        label.set_x(linepricelabel, bar_index)
        box.set_right(array.get(bullboxarray, i), bar_index)
        crossed = low < line.get_y1(templowerline)
        if crossed and not tested
            array.set(lowerzonetestedarr, i, true)
            label.delete(linepricelabel)
        if crossalerts and crossed and not tested
            array.set(lowerzonetestedarr, i, true)
            label.delete(linepricelabel)
            alert('bottom Line Has Been Crossed! At ' + str.tostring(close), alert.freq_once_per_bar)
        else if not tested
            line.set_x2(tempupperline, bar_index)
            array.set(lowerphzonearr, i, tempupperline)
            line.set_x2(templowerline, bar_index)
            array.set(lowerplzonearr, i, templowerline)

//Plotting

plotshape(showlabels ? bearob : na, title='Bearish OrderBlock', style=shape.triangledown, location=location.abovebar, color=color.new(highzonescolor, 0), size=size.tiny, offset=po)
plotshape(showlabels ? bullob : na, title='Bullish OrderBlock', style=shape.triangleup,   location=location.belowbar, color=color.new(lowzonescolor,  0), size=size.tiny, offset=po)
barcolor(obc, offset=po)

//Alerts

if obalerts and bearob == 1
    alert('Bearish OB Detected! At ' + str.tostring(bearobprice), alert.freq_once_per_bar)
if obalerts and bullob == 1
    alert('Bullish OB Detected! At ' + str.tostring(bullobprice), alert.freq_once_per_bar)