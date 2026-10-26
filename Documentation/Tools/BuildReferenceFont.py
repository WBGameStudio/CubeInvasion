from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from fontTools.pens.ttGlyphPen import TTGlyphPen
from pathlib import Path
import argparse
parser=argparse.ArgumentParser();parser.add_argument('source');parser.add_argument('output');args=parser.parse_args()
font=instantiateVariableFont(TTFont(args.source),{'wght':700,'wdth':90},inplace=False)
glyf=font['glyf'];order=font.getGlyphOrder()[:]
for character,base,accent,below in [('Ğ','G','breve',False),('ğ','g','breve',False),('İ','I','dotaccent',False),('Ş','S','cedilla',True),('ş','s','cedilla',True)]:
 name='uni%04X'%ord(character);a=glyf[base];b=glyf[accent];a.recalcBounds(glyf);b.recalcBounds(glyf)
 dx=round((a.xMin+a.xMax-b.xMin-b.xMax)/2);dy=a.yMin-12-b.yMax if below else a.yMax+35-b.yMin
 pen=TTGlyphPen(font.getGlyphSet());pen.addComponent(base,(1,0,0,1,0,0));pen.addComponent(accent,(1,0,0,1,dx,dy));glyf[name]=pen.glyph();font['hmtx'].metrics[name]=font['hmtx'].metrics[base];order.append(name)
 for table in font['cmap'].tables:
  if table.isUnicode():table.cmap[ord(character)]=name
font.setGlyphOrder(order)
for rec in font['name'].names:
 if rec.nameID in (1,2,4,6,16,17):
  text={1:'Cube Rounded',2:'Bold',4:'Cube Rounded Bold',6:'CubeRounded-Bold',16:'Cube Rounded',17:'Bold'}[rec.nameID];rec.string=text.encode(rec.getEncoding(),errors='replace')
font.save(args.output)
assert all(ord(c) in font.getBestCmap() for c in 'ÇĞİÖŞÜçğıöşü')
print('Cube Rounded static font with Turkish composites verified')
