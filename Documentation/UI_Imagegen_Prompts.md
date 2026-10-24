# Imagegen üretim kaydı

Mod: yerleşik `image_gen` aracı. CLI/API fallback kullanılmadı. İki çıktı incelendi ve oyunda kullanılan varlıklar olarak projeye kopyalandı. Görseller tam arka plan illüstrasyonudur; metin ve tıklanabilir kontroller Unity'de oluşturulur.

## Ana menü

Kaydedilen çıktı: `/Users/furkanelmali/Documents/GitHub/CubeInvasion/Assets/CubeInvasion/Art/UI/MenuHero.png`

Son prompt (verbatim):

> Use case: stylized-concept. Asset type: production portrait background illustration for the Unity mobile game Cube Invasion main menu. Create a polished premium toy-like sci-fi arcade illustration, portrait 2:3 composition. A small cobalt blue beveled robot scout with cyan visor and compact pulse pistol stands on a floating square arena platform, facing a distant cluster of floating cube islands in a dark navy void. Broad friendly geometric silhouettes, matte plastic and graphite metal, cyan edge lights with a few warm amber details, soft cinematic upper-left lighting. The robot and platform occupy the upper 55 percent, centered slightly right, with elegant atmospheric depth. Lower 40 percent fades smoothly to almost flat dark navy (#0b1224), intentionally quiet for real interactive UI buttons placed by the game. Top 12 percent also dark and quiet for the title. Attractive clean stylized 3D game key art, crisp silhouettes, restrained bloom. No text, no letters, no logos, no watermark, no buttons, no frames, no UI screenshot. Deliver the illustration itself as a single full-bleed background.

## Bölüm yolu

Kaydedilen çıktı: `/Users/furkanelmali/Documents/GitHub/CubeInvasion/Assets/CubeInvasion/Art/UI/CampaignIslands.png`

Son prompt (verbatim):

> Use case: stylized-concept. Asset type: production portrait background illustration for a mobile game campaign road-map UI, Cube Invasion. Single full-bleed 2:3 portrait background, no UI screenshot. A winding vertical journey through floating toy sci-fi cube islands in deep midnight navy space. Isometric beveled square islands and subtle atmospheric clouds populate the far left and far right margins; center 60 percent stays mostly empty dark navy so real interactive level nodes and a curved glowing path can be overlaid by the game engine. Bottom region hints at cyan industrial islands, middle edges have violet crystal/machinery, upper edges warm amber fortress cubes. Elegant premium mobile arcade 3D illustration, matching a cobalt scout robot / cyan energy toy sci-fi game, restrained cyan glows, soft upper-left lighting, high quality matte plastic and graphite, atmospheric depth, uncluttered composition. The illustration should remain dark and low contrast behind UI text. No text, numbers, letters, logos, buttons, paths, level markers, borders or watermark. Keep the central corridor quiet all the way from bottom to top.

Silah ve Scout önizlemeleri Imagegen çıktısı değildir; mevcut proje modellerinin Unity ile render edilmesidir. Yuvarlak paneller/düğümler de engine-native sprite varlıklarıdır. Bu sayede görseller ile gerçek kontrol/ilerleme verileri birbirinden ayrıdır.
