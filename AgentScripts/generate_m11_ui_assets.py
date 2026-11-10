"""Generate original M11 UI sprites. Requires Pillow; run from repository root."""
from pathlib import Path
from PIL import Image, ImageDraw

OUT = Path(__file__).resolve().parents[1] / 'Assets/_Project/Art/UI/M11'
S = 4
INK = (247, 244, 239, 255)


def canvas(w, h):
    return Image.new('RGBA', (w*S, h*S), (0, 0, 0, 0))


def save(image, name, w, h):
    OUT.mkdir(parents=True, exist_ok=True)
    image.resize((w, h), Image.Resampling.LANCZOS).save(OUT / (name + '.png'), optimize=False)


def panel(name, w, h, radius, fill, border):
    image = canvas(w, h)
    d = ImageDraw.Draw(image)
    d.rounded_rectangle((2*S, 2*S, (w-3)*S, (h-3)*S), radius*S, fill=fill, outline=border, width=2*S)
    save(image, name, w, h)


def icon(name, draw):
    image = canvas(96, 96)
    d = ImageDraw.Draw(image)
    def line(points, width=5, color=INK):
        d.line([(int(x*S), int(y*S)) for x, y in points], fill=color, width=width*S, joint='curve')
    def ellipse(box, width=5):
        d.ellipse(tuple(int(x*S) for x in box), outline=INK, width=width*S)
    draw(line, ellipse)
    save(image, 'icon_' + name, 96, 96)


def main():
    panel('dialogue_frame', 96, 96, 12, (37,43,50,232), (170,181,189,190))
    panel('nameplate', 96, 64, 9, (37,43,50,246), (229,202,162,220))
    panel('button_background', 64, 64, 11, (45,53,61,205), (170,181,189,145))
    panel('tooltip_background', 64, 64, 10, (30,37,42,248), (129,187,183,210))
    marker = canvas(48, 48)
    ImageDraw.Draw(marker).polygon([(13*S,18*S),(35*S,18*S),(24*S,30*S)], fill=(229,202,162,255))
    save(marker, 'continue_marker', 48, 48)
    icon('next', lambda l,e: (l([(30,23),(60,48),(30,73)]), l([(67,23),(67,73)])))
    icon('hide', lambda l,e: (e((17,30,79,66)), e((39,39,57,57)), l([(21,75),(75,21)])))
    icon('backlog', lambda l,e: (l([(24,25),(72,25),(72,64),(50,64),(35,75),(35,64),(24,64),(24,25)]), l([(33,38),(63,38)],3), l([(33,49),(57,49)],3)))
    icon('skip', lambda l,e: (l([(20,25),(45,48),(20,71),(20,25)]), l([(45,25),(70,48),(45,71),(45,25)]), l([(77,25),(77,71)])))
    icon('auto', lambda l,e: (l([(27,67),(43,27),(53,27),(69,67)]), l([(34,52),(62,52)]), l([(75,37),(75,62)],3)))
    icon('save', lambda l,e: (l([(22,21),(68,21),(75,28),(75,75),(22,75),(22,21)]), l([(31,22),(31,40),(61,40),(61,22)]), l([(32,74),(32,52),(65,52),(65,74)])))
    icon('load', lambda l,e: (l([(20,35),(40,35),(47,42),(76,42),(70,71),(19,71),(20,35)]), l([(26,35),(26,27),(68,27),(68,41)])))
    icon('settings', lambda l,e: (e((26,26,70,70)), e((40,40,56,56)), l([(48,14),(48,25)]), l([(48,71),(48,82)]), l([(14,48),(25,48)]), l([(71,48),(82,48)]), l([(24,24),(32,32)]), l([(64,64),(72,72)]), l([(24,72),(32,64)]), l([(64,32),(72,24)])))
    icon('records', lambda l,e: (l([(23,23),(42,23),(48,28),(54,23),(73,23),(73,72),(54,72),(48,77),(42,72),(23,72),(23,23)]), l([(48,28),(48,77)],3), l([(30,35),(40,35)],3), l([(56,35),(66,35)],3)))


if __name__ == '__main__':
    main()
