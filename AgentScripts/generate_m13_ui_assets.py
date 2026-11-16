from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import math, random, sys

out = Path(sys.argv[1]) if len(sys.argv)>1 else Path('Assets/_Project/Art/UI/M13')
out.mkdir(parents=True, exist_ok=True)
# Original deterministic material fields: small amplitudes keep text surfaces calm.
roles = {
 'panel': ((31,39,47), 254, 15), 'secondary': ((39,48,56), 250, 12),
 'card': ((48,58,65), 250, 12), 'inset': ((23,30,36), 252, 10),
 'button': ((55,68,74), 255, 10), 'selected': ((60,91,90),255,10),
 'tab': ((43,53,60),255,9), 'track': ((21,28,33),255,8),
 'handle': ((130,178,174),255,8), 'focus': ((229,202,162),255,8),
}
for direction in ('A','B'):
 for role,(base,alpha,radius) in roles.items():
  n=128; rng=random.Random('M13-'+direction+'-'+role)
  im=Image.new('RGBA',(n,n)); pix=im.load()
  for y in range(n):
   for x in range(n):
    # fine material variation with restrained spatial lighting
    noise=rng.gauss(0, 0.22 if 20 < x < 108 and 20 < y < 108 else (0.7 if direction=='A' else 1.0))
    light=(1-y/(n-1))*(3.0 if direction=='A' else 1.4)
    if direction=='B': light += math.sin(y*0.11)*0.25
    warm=(9,5,0) if direction=='B' else (0,0,0)
    pix[x,y]=tuple(max(0,min(255,round(base[i]+warm[i]+noise+light))) for i in range(3))+(alpha,)
  radius = radius if direction == 'A' else max(4, radius-5)
  mask=Image.new('L',(n,n)); d=ImageDraw.Draw(mask); d.rounded_rectangle((1,1,n-2,n-2),radius,fill=255)
  im.putalpha(Image.composite(im.getchannel('A'),Image.new('L',(n,n),0),mask))
  d=ImageDraw.Draw(im)
  edge=(156,180,184,105) if direction=='A' else (190,170,144,110)
  d.rounded_rectangle((1,1,n-2,n-2),radius,outline=edge,width=1)
  d.line((radius+2,3,n-radius-3,3),fill=(225,231,226,35) if direction=='A' else (235,216,187,48),width=1)
  if role=='focus':
   im=Image.new('RGBA',(n,n)); d=ImageDraw.Draw(im); d.rounded_rectangle((2,2,n-3,n-3),radius,outline=(229,202,162,255),width=3)
  im.save(out/f'{direction}_{role}.png', optimize=False, compress_level=9)
for direction in ('A','B'):
 rng=random.Random('M13-grain-'+direction)
 grain=Image.new('RGBA',(128,128)); px=grain.load()
 for y in range(128):
  for x in range(128):
   v=rng.randrange(2); px[x,y]=((255,255,255,4) if v else (0,0,0,4 if direction=='A' else 7))
 grain.save(out/f'{direction}_grain.png',optimize=False,compress_level=9)
print('generated',len(roles)*2+2,'assets in',out)
