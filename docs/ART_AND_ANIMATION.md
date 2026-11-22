# Оформление и анимации — 18 ноября 2026

Ориентир — предоставленные пользователем изображения дома и персонажей. Реализована интерпретация в игровом 3D: два основных этажа, отдельный подвал, верхняя комната с открытыми стропилами, кухня, гостиная с камином и телевизором, спальни с железными кроватями, детская, кабинет, ванная, лестничные перила, крыльцо и лесной двор. Это не точное воспроизведение фотореалистичных референсов. Мебель и персонажи построены из собственных мешей и геометрии Unity; персонажи стилизованы и требуют дальнейшей художественной работы.

Пробуждение длится 7,8 секунды: герой лежит на кровати, садится, встаёт, камера переходит к управлению от первого лица. Герой просыпается в начале каждого из трёх игровых дней. После третьей поимки затемнение переходит сразу на экран поражения. Побег начинается на крыльце. Игрок видит, как его руки движутся в такт шагам по лесной тропе. После бега камера возвращается к лицу смотрителя, затем звучит прощальная реплика и появляется THE END. Во время этих сцен и мини-игр враг остановлен.

## Материалы

Текстуры дерева, пола, штукатурки, камня и крыши скачаны с Poly Haven, лицензия CC0: https://polyhaven.com/license . Файлы, исходные URL и контрольные суммы перечислены в ASSET_SOURCES.json. Используются цветовые карты и normal maps до 2048 px. Эти материалы входят в проект и не требуют сети во время игры.

FamilyPortraits.png — оригинальный атлас вымышленных семейных портретов, создан встроенным image_gen. Файл: Assets/Resources/Art/FamilyPortraits.png. Unity выбирает четверти атласа через UV; отдельные копии изображений не нужны.

Итоговый промпт image_gen:

> Use case: historical-scene. Asset type: an original 2x2 texture atlas of four old family oil paintings, for frames in a 3D abandoned-house horror game. Exactly four rectangular paintings arranged as a precise equal 2x2 grid, no gutters, no decorative frames, each fills its full quadrant. Top-left: solemn elderly man in dark 1890s suit. Top-right: pale adult woman with tied hair and black high collar. Bottom-left: serious young boy in an old gray shirt. Bottom-right: seated adult couple in a dim room. All fictional people, no recognizable real person. Frontal bust portraits with faces centered, realistic muted ochre/sepia and charcoal palette, distressed faded canvas, small cracks and grime, deep warm black backgrounds. Rich but subdued period oil painting detail, uneven aged varnish. Orthographic flat albedo image only; no room scene, no outer border, no lettering, no graphic horror or gore. 2048x2048 square, each quadrant remains a complete stand-alone portrait.

## Редактирование

Открывать Assets/Scenes/House.unity. Дом виден в редакторе до Play. Скрипты ReferenceArt.cs и ReferenceDetails.cs формируют оформление; HouseBuilder.cs — базовую геометрию, интерактивные предметы и свет. StoryDirector.cs управляет кат-сценами, CharacterRig.cs — частями тела.

Команда TISHE / Rebuild reference house scene заново создаёт сцену из кода. Ручные изменения сцены нужно сначала сохранить под другим именем. Предыдущая версия исходников сохранена в Backups/before-reference-redesign.zip.

Ограничения: нет профессионально скульптированных и скиненных персонажей, лицевой анимации или озвучки. Лес декоративный, а не отдельный открытый уровень. Верхняя комната со стропилами находится на втором этаже. Размеры основного корпуса сохранены ради рабочей навигации; подвал расширяет доступную карту.

## Облик смотрителя и мебель

KeeperFace.png — созданная с помощью ImageGen текстура лица вымышленного персонажа. KeeperHead.cs строит сетку по контуру черепа и совмещает глазницы, скулы, нос и подбородок с текстурой. При поимке включается боковой свет. Модель игрока отбрасывает тень головы при ходьбе и пробуждении.

Модели прикроватной тумбочки, шарнирной лампы и обеденного стола получены с Poly Haven. Источники и хеши входят в FURNITURE_SOURCES.json.

Лесной двор использует фактуру коры, хвою, папоротник и почву из библиотеки Poly Haven под лицензией CC0. Источники и контрольные суммы перечислены в FOREST_SOURCES.json. Для смеха у шкафа и прощания с Берекетом записана русская синтезированная речь голосом Pavel.

## Kitchen and nursery furnishings

RoomHeirlooms models the spindle bed, hanging mobile, wooden train, kettle, stoneware and table settings. The mobile moves independently of the static furnishings. The kitchen pendant uses one short-range light without a shadow map. The fabric, drawings and ceramics share one original image in Assets/Resources/Art/HouseHeirlooms.png. It was created using the built-in ImageGen tool from this prompt.

> A single square albedo texture atlas for an original atmospheric old-house video game. Exactly four edge-to-edge equal square quadrants with hard straight boundaries at 50 percent. Orthographic flat scan, no background or framing, no gutters. TOP LEFT original child’s wax crayon drawing of a little ochre house, black pine trees, and a single tall distant stick figure on yellowed ruled school paper. TOP RIGHT aged kitchen recipe paper: charming faded 1940s engraving of pear preserves, pears, leaves and enamel pot, faint ruled pencil marks, no legible text. BOTTOM LEFT full-bleed faded patchwork quilt fabric photographed straight down, many 8-point star quilt squares, muted brick red, ochre, ivory and faded indigo, visible stitching and woven fibres, no folds, no perspective. BOTTOM RIGHT antique all-over ceramic kitchen tile pattern: grid of 4x4 worn warm ivory squares with hand painted cobalt blue stylized floral motifs, thin sandy grout, gentle crazing. Make each quadrant fill its exact area to all four edges, no perspective, no ambient shadows, original unbranded pieces.

## Wall instruments and hearth

WallInstruments builds five wall-mounted devices and the five loose items. LivingRoomEffects supplies the log fire, local crackling, curved CRT screen and quiet television hiss. The CRT receives power when the fuse circuit is completed. The original surface atlas is Assets/Resources/Art/InstrumentPlates.png. Its built-in ImageGen prompt was:

> Create one professional PBR game albedo atlas, landscape 3:2 image, strictly 3 columns by 2 rows of equal SQUARE cells. Fill the exact full frame edge-to-edge, hard borders no gaps. Flat orthographic scanned textures, absolutely no perspective, no objects protruding, no directional light or environmental background. The top left cell: peeling dark bottle-green painted industrial electrical switchboard metal with restrained old ivory schematic wiring lines near edges, small rectangular aged warning label upper left, chipped corners and rust stains, centre largely empty for real 3D gauge. Top middle: worn dark bronze and walnut antique four-wheel mechanical lock escutcheon, fine engraved borders and index marks, broad clear center for physical wheels. Top right: antique dark walnut and worn burgundy lithographed faceplate of a coin-operated mechanical game, elegant gold ornamental pinstripes and vintage starbursts along borders, three little cream stars along bottom, central space vacant for physical tic tac toe board. Bottom left: blue-grey industrial air machinery enamel with aged drafting linework, tiny cream safety placard, colorful restrained pipe diagrams near border, center vacant. Bottom middle: antique brown mid-century radio dial escutcheon on patinated fine metal, thin engraved gold concentric wave motifs around borders, a little maker's badge in lower corner, clear center for physical knobs. Bottom right: authentic dark charcoal soot blackened small firebricks set in horizontal running bond rows, heavy sooty powder near top and reddish burnt bricks lower rows. All elements original, analog historical aesthetic, realistically photographed material wear, controlled fine detail. NO mockup, no pre-rendered knobs, switches, instruments, titles or faux modern typography. Exactly 6 distinct square material regions.

## Basement photography and night lighting

SafelightRooms places a ruby darkroom lamp above four drying prints, as well as protected lamps at the stair, hall and exit. The shelf and props are decorative, with no collision geometry. Light spill is distance-limited and shadowless. The contact sheet is an original built-in ImageGen texture saved in Assets/Resources/Art/DarkroomPrints.png.

> An ORIGINAL production texture atlas of exactly FOUR different photographic silver gelatin prints, a 2 by 2 grid of equal square photographs, straight-on flatbed scan. Each quadrant fills EXACTLY one quarter, each has its own tiny off-white darkroom easel border, no gaps between quarters, no background, no labels or lettering, no people except the one distant silhouette specified. Telling one unsettling story in an old wooden rural house. Upper left: exterior of a plain isolated two-story timber house and dark pine trees at dusk, a single upper window is bright white. Upper right: inside a child's modest old room with a wooden spindle crib and a wardrobe, dim moonlight, empty. Lower left: photo of the very same wardrobe from closer, its door has opened just a tiny crack, uncanny empty dark space; large clear silhouettes. Lower right: muddy trail through pine woods photographed by direct flash, far between the trees a small indistinct elderly man's silhouette looking back. Strong but readable silvery midtones, authentic slightly distressed fiber paper, subtle fingerprints, large clear compositions that read in a small game prop. Vintage documentary analog photography, high fidelity, restrained and natural; no collage overlapping, no extra panels, absolutely no graphic violence or text.

The main menu uses original artwork generated with OpenAI ImageGen, stored in Assets/Resources/Menu/KeeperHouse.png. The art direction was a two-storey timber keeper’s house at wet blue hour, warm porch light and a muted red upstairs window, with dark spruce on the right for live Kazakh typography.

The three day titles share an original ImageGen close-up of a worn wooden door and brass lock in Assets/Resources/Menu/WakeDoor.png. WakeDayTitle draws the Kazakh lettering and reveals one additional cut in the wood at each awakening.

The attic pantry uses original ImageGen illustrations in Assets/Resources/Art/PantryPrints.png for the biscuit tins, tea tins, crate labels and herb print. Its oak dresser, glazed ware, linen runner and brass candlestick are assembled in RoomHeirlooms.Pantry.cs.

The five FamilyTape/Memory clips are original montages of the existing Kazakh keeper and player recordings with household foley, an original three-note motif and filtered tape hiss. Memory5 combines the name from EscapeFarewell with keeper_3. ReceiverCall is a separate synthesized diegetic sound.
