using RestaurantService.Models;

namespace RestaurantService.Data
{
    public static class SeedData
    {
        public static void Initialize(RestaurantDbContext context)
        {
            if (context.Restaurants.Any()) return;

            // ── Owner GUIDs (would normally come from UserService) ──────────────
            var owner1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var owner2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var owner3 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var owner4 = Guid.Parse("44444444-4444-4444-4444-444444444444");

            // ── Categories ───────────────────────────────────────────────────────
            var catStarters   = new MenuCategory { Name = "Starters",      Description = "Appetisers and small plates" };
            var catMainCourse = new MenuCategory { Name = "Main Course",    Description = "Signature mains" };
            var catBreads     = new MenuCategory { Name = "Breads & Rice",  Description = "Artisan breads and fragrant rice" };
            var catDesserts   = new MenuCategory { Name = "Desserts",       Description = "Indulgent sweet finishes" };
            var catDrinks     = new MenuCategory { Name = "Beverages",      Description = "Curated drinks and mocktails" };
            var catSushi      = new MenuCategory { Name = "Sushi & Rolls",  Description = "Premium nigiri, sashimi and rolls" };
            var catGrills     = new MenuCategory { Name = "Grills & Kebabs",Description = "Tandoor and charcoal grills" };
            var catPasta      = new MenuCategory { Name = "Pasta & Risotto",Description = "Handmade pasta and slow-cooked risotto" };

            context.MenuCategories.AddRange(
                catStarters, catMainCourse, catBreads, catDesserts, catDrinks,
                catSushi, catGrills, catPasta);
            context.SaveChanges();

            // ════════════════════════════════════════════════════════════════════
            // RESTAURANT 1 — Maharaja's Table  (North Indian fine-dining)
            // ════════════════════════════════════════════════════════════════════
            var r1Id = Guid.NewGuid();
            var r1 = new Restaurant
            {
                Id             = r1Id,
                OwnerId        = owner1,
                Name           = "Maharaja's Table",
                Description    = "An opulent North Indian fine-dining experience inspired by royal Mughal kitchens. Every dish is crafted with hand-ground spices, slow-cooked in copper handi, and finished with edible gold leaf.",
                Address        = "12 Regal Boulevard, Connaught Place, New Delhi 110001",
                CuisineTypes   = "North Indian, Mughlai, Awadhi",
                LogoUrl        = "https://images.unsplash.com/photo-1585937421612-70a008356fbe?w=400",
                ApprovalStatus = "Approved",
                IsOpen         = true,
                CreatedAt      = DateTime.UtcNow
            };

            var r1Menu = new List<MenuItem>
            {
                new() { RestaurantId = r1Id, Name = "Gilded Seekh Kebab",         Description = "Minced lamb with saffron, cardamom and rose water, finished with edible silver leaf. Served with mint chutney.",                                    Price = 1599m,  ImageUrl = "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catStarters },
                new() { RestaurantId = r1Id, Name = "Truffle Malai Tikka",         Description = "Boneless chicken marinated in truffle-infused cream, hung curd and aged cheddar. Slow-roasted in a clay tandoor.",                                  Price = 1799m,  ImageUrl = "https://images.unsplash.com/photo-1567188040759-fb8a883dc6d8?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catStarters },
                new() { RestaurantId = r1Id, Name = "Paneer Zafrani Tikka",        Description = "Thick-cut cottage cheese marinated in saffron, cream and Kashmiri chilli. Charred to perfection in the tandoor.",                                   Price = 1399m,  ImageUrl = "https://images.unsplash.com/photo-1631452180519-c014fe946bc7?w=600", IsAvailable = true, DietType = "Veg",     Category = catStarters },
                new() { RestaurantId = r1Id, Name = "Raan-e-Maharaja",             Description = "Whole slow-roasted leg of lamb marinated for 24 hours in yoghurt, raw papaya and 22 spices. Carved tableside.",                                     Price = 4499m,  ImageUrl = "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },
                new() { RestaurantId = r1Id, Name = "Dal Maharani",                Description = "Black lentils slow-cooked on a wood fire for 18 hours with hand-churned butter and single-origin cream. A royal heirloom recipe.",                   Price = 1249m,  ImageUrl = "https://images.unsplash.com/photo-1546833999-b9f581a1996d?w=600", IsAvailable = true, DietType = "Veg",     Category = catMainCourse },
                new() { RestaurantId = r1Id, Name = "Butter Chicken Royale",       Description = "Free-range chicken in a velvety tomato-cashew gravy enriched with Amul butter and Kashmiri saffron. The gold standard.",                            Price = 1899m,  ImageUrl = "https://images.unsplash.com/photo-1603894584373-5ac82b2ae398?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },
                new() { RestaurantId = r1Id, Name = "Warqi Paratha",               Description = "Flaky layered bread made with pure ghee, baked in a stone oven. Served warm with white butter.",                                                     Price = 599m,   ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600", IsAvailable = true, DietType = "Veg",     Category = catBreads },
                new() { RestaurantId = r1Id, Name = "Saffron Biryani",             Description = "Aged Basmati rice layered with slow-cooked lamb, caramelised onions and Kashmiri saffron. Sealed and dum-cooked in a clay pot.",                     Price = 2299m,  ImageUrl = "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catBreads },
                new() { RestaurantId = r1Id, Name = "Gulab Jamun Cheesecake",      Description = "A fusion of classic gulab jamun and New York cheesecake. Rose-water glaze, pistachio crumble and gold dust.",                                        Price = 1099m,  ImageUrl = "https://images.unsplash.com/photo-1551024506-0bccd828d307?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },
                new() { RestaurantId = r1Id, Name = "Kesar Kulfi Trio",            Description = "Three artisan kulfis — saffron-pistachio, rose-cardamom and mango-chilli — served on a silver platter with falooda.",                               Price = 899m,   ImageUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },
                new() { RestaurantId = r1Id, Name = "Royal Thandai",               Description = "Chilled milk infused with almonds, fennel, rose petals and saffron. A Holi classic elevated to fine-dining.",                                        Price = 749m,   ImageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=600", IsAvailable = true, DietType = "Veg",     Category = catDrinks },
            };

            // ════════════════════════════════════════════════════════════════════
            // RESTAURANT 2 — Sakura Omakase  (Japanese premium)
            // ════════════════════════════════════════════════════════════════════
            var r2Id = Guid.NewGuid();
            var r2 = new Restaurant
            {
                Id             = r2Id,
                OwnerId        = owner2,
                Name           = "Sakura Omakase",
                Description    = "An intimate 20-seat Japanese omakase counter where Chef Kenji Mori presents a seasonal 12-course tasting menu. Ingredients flown in from Tsukiji market twice weekly.",
                Address        = "7 Zen Garden Lane, Bandra West, Mumbai 400050",
                CuisineTypes   = "Japanese, Omakase, Sushi",
                LogoUrl        = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=400",
                ApprovalStatus = "Approved",
                IsOpen         = true,
                CreatedAt      = DateTime.UtcNow
            };

            var r2Menu = new List<MenuItem>
            {
                new() { RestaurantId = r2Id, Name = "Otoro Nigiri",                Description = "Premium fatty tuna belly from Bluefin, lightly seasoned with yuzu zest and micro shiso. Two pieces per serving.",                                   Price = 2699m,  ImageUrl = "https://images.unsplash.com/photo-1617196034183-421b4040ed20?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catSushi },
                new() { RestaurantId = r2Id, Name = "Wagyu Aburi Roll",            Description = "A5 Wagyu beef torched over a premium California roll, finished with truffle ponzu and crispy shallots.",                                            Price = 3199m,  ImageUrl = "https://images.unsplash.com/photo-1562802378-063ec186a863?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catSushi },
                new() { RestaurantId = r2Id, Name = "Uni & Ikura Gunkan",          Description = "Sea urchin and salmon roe on hand-pressed shari rice, wrapped in premium nori. Ocean freshness in every bite.",                                     Price = 2349m,  ImageUrl = "https://images.unsplash.com/photo-1611143669185-af224c5e3252?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catSushi },
                new() { RestaurantId = r2Id, Name = "Truffle Edamame",             Description = "Steamed edamame tossed in black truffle oil, Maldon sea salt and togarashi. A luxurious take on a Japanese classic.",                               Price = 999m,   ImageUrl = "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=600", IsAvailable = true, DietType = "Veg",     Category = catStarters },
                new() { RestaurantId = r2Id, Name = "Miso Black Cod",              Description = "Nobu-style black cod marinated in white miso and mirin for 72 hours, broiled to caramelised perfection. Served with pickled ginger.",               Price = 3649m,  ImageUrl = "https://images.unsplash.com/photo-1559847844-5315695dadae?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },
                new() { RestaurantId = r2Id, Name = "Wagyu Sukiyaki",              Description = "A5 Wagyu beef hot pot with tofu, enoki mushrooms and glass noodles in a sweet soy dashi broth. Served with raw egg dip.",                           Price = 5199m,  ImageUrl = "https://images.unsplash.com/photo-1547592180-85f173990554?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },
                new() { RestaurantId = r2Id, Name = "Matcha Tiramisu",             Description = "A Japanese-Italian fusion — ceremonial grade matcha mascarpone layered with yuzu-soaked ladyfingers and white chocolate shavings.",                  Price = 1149m,  ImageUrl = "https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },
                new() { RestaurantId = r2Id, Name = "Yuzu Sake Spritz",            Description = "Premium junmai sake with fresh yuzu juice, elderflower tonic and a sprig of shiso. Served over hand-carved ice.",                                    Price = 1349m,  ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=600", IsAvailable = true, DietType = "Veg",     Category = catDrinks },
            };

            // ════════════════════════════════════════════════════════════════════
            // RESTAURANT 3 — Ember & Spice  (Modern Indian BBQ / Kebab house)
            // ════════════════════════════════════════════════════════════════════
            var r3Id = Guid.NewGuid();
            var r3 = new Restaurant
            {
                Id             = r3Id,
                OwnerId        = owner3,
                Name           = "Ember & Spice",
                Description    = "A contemporary Indian grill house celebrating the art of live-fire cooking. Our charcoal pits and clay tandoors produce smoky, bold flavours rooted in Punjabi and Rajasthani traditions.",
                Address        = "88 Flame Street, Koramangala, Bengaluru 560034",
                CuisineTypes   = "Indian BBQ, Punjabi, Rajasthani",
                LogoUrl        = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=400",
                ApprovalStatus = "Approved",
                IsOpen         = true,
                CreatedAt      = DateTime.UtcNow
            };

            var r3Menu = new List<MenuItem>
            {
                new() { RestaurantId = r3Id, Name = "Peshwari Lamb Chops",         Description = "New Zealand lamb chops marinated in raw papaya, garam masala and mustard oil. Charcoal-grilled and served with pickled onions.",                    Price = 2849m,  ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catGrills },
                new() { RestaurantId = r3Id, Name = "Achari Paneer Tikka",         Description = "Cottage cheese cubes marinated in a tangy pickle masala with mustard seeds and fenugreek. Grilled in the tandoor.",                                 Price = 1449m,  ImageUrl = "https://images.unsplash.com/photo-1631452180519-c014fe946bc7?w=600", IsAvailable = true, DietType = "Veg",     Category = catGrills },
                new() { RestaurantId = r3Id, Name = "Smoked Boti Kebab",           Description = "Tender lamb cubes slow-smoked over hickory wood, marinated in yoghurt, cloves and black cardamom. Served with roomali roti.",                       Price = 2149m,  ImageUrl = "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catGrills },
                new() { RestaurantId = r3Id, Name = "Laal Maas",                   Description = "Rajasthani fiery red mutton curry cooked with mathania chillies, whole spices and pure ghee. A warrior's feast.",                                   Price = 2499m,  ImageUrl = "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },
                new() { RestaurantId = r3Id, Name = "Sarson da Saag & Makki Roti", Description = "Slow-cooked mustard greens with ginger and garlic, served with hand-pressed corn flatbread and a dollop of white butter.",                          Price = 1399m,  ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600", IsAvailable = true, DietType = "Veg",     Category = catMainCourse },
                new() { RestaurantId = r3Id, Name = "Tandoori Jhinga",             Description = "Jumbo tiger prawns marinated in ajwain, turmeric and cream, roasted in the tandoor. Served with garlic butter and lemon.",                          Price = 2999m,  ImageUrl = "https://images.unsplash.com/photo-1559847844-5315695dadae?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catGrills },
                new() { RestaurantId = r3Id, Name = "Missi Roti",                  Description = "Whole wheat and chickpea flour flatbread with carom seeds, green chilli and fresh coriander. Baked in the tandoor.",                                 Price = 499m,   ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600", IsAvailable = true, DietType = "Veg",     Category = catBreads },
                new() { RestaurantId = r3Id, Name = "Shahi Tukda",                 Description = "Fried bread soaked in saffron-infused rabri, garnished with silver leaf, rose petals and crushed pistachios.",                                       Price = 949m,   ImageUrl = "https://images.unsplash.com/photo-1551024506-0bccd828d307?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },
                new() { RestaurantId = r3Id, Name = "Masala Chaas",                Description = "Chilled buttermilk tempered with cumin, black salt, ginger and fresh mint. The perfect palate cleanser.",                                           Price = 449m,   ImageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=600", IsAvailable = true, DietType = "Veg",     Category = catDrinks },
            };

            // ════════════════════════════════════════════════════════════════════
            // RESTAURANT 4 — Trattoria Nobile  (Italian fine-dining)
            // ════════════════════════════════════════════════════════════════════
            var r4Id = Guid.NewGuid();
            var r4 = new Restaurant
            {
                Id             = r4Id,
                OwnerId        = owner4,
                Name           = "Trattoria Nobile",
                Description    = "An authentic Italian fine-dining experience in the heart of the city. Chef Marco Rossi brings the flavours of Tuscany and Amalfi to your table — handmade pasta, wood-fired pizzas and an award-winning wine cellar.",
                Address        = "5 Via Roma, Jubilee Hills, Hyderabad 500033",
                CuisineTypes   = "Italian, Mediterranean, Continental",
                LogoUrl        = "https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=400",
                ApprovalStatus = "Approved",
                IsOpen         = true,
                CreatedAt      = DateTime.UtcNow
            };

            var r4Menu = new List<MenuItem>
            {
                new() { RestaurantId = r4Id, Name = "Burrata con Tartufo",         Description = "Creamy burrata from Puglia served with shaved black truffle, heirloom tomatoes, basil oil and toasted sourdough.",                                   Price = 1849m,  ImageUrl = "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=600", IsAvailable = true, DietType = "Veg",     Category = catStarters },
                new() { RestaurantId = r4Id, Name = "Carpaccio di Manzo",          Description = "Paper-thin Wagyu beef carpaccio with rocket, Parmigiano Reggiano, capers and a lemon-truffle dressing.",                                            Price = 2149m,  ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catStarters },
                new() { RestaurantId = r4Id, Name = "Tagliatelle al Ragù",         Description = "Hand-rolled egg tagliatelle with a 6-hour slow-cooked Bolognese of veal, pork and Chianti. Finished with aged Parmigiano.",                         Price = 2349m,  ImageUrl = "https://images.unsplash.com/photo-1555949258-eb67b1ef0ceb?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catPasta },
                new() { RestaurantId = r4Id, Name = "Risotto al Tartufo Nero",     Description = "Carnaroli rice slow-cooked in white wine and Parmigiano broth, finished with generous shavings of Périgord black truffle and aged butter.",         Price = 2999m,  ImageUrl = "https://images.unsplash.com/photo-1476124369491-e7addf5db371?w=600", IsAvailable = true, DietType = "Veg",     Category = catPasta },
                new() { RestaurantId = r4Id, Name = "Lobster Linguine",            Description = "Fresh linguine tossed with half a Boston lobster, cherry tomatoes, white wine, garlic and Calabrian chilli. A coastal Italian classic.",             Price = 3999m,  ImageUrl = "https://images.unsplash.com/photo-1559847844-5315695dadae?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catPasta },
                new() { RestaurantId = r4Id, Name = "Bistecca alla Fiorentina",    Description = "1.2kg T-bone of Chianina beef, dry-aged 45 days, grilled over olive wood. Served with rosemary roasted potatoes and salsa verde.",                  Price = 6499m,  ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },
                new() { RestaurantId = r4Id, Name = "Parmigiana di Melanzane",     Description = "Layers of fried aubergine, San Marzano tomato sauce, fior di latte and fresh basil. Baked until golden and bubbling.",                              Price = 1599m,  ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600", IsAvailable = true, DietType = "Veg",     Category = catMainCourse },
                new() { RestaurantId = r4Id, Name = "Tiramisu della Casa",         Description = "The house tiramisu — Savoiardi soaked in single-origin espresso and Marsala, layered with mascarpone cream and dusted with Valrhona cocoa.",         Price = 1099m,  ImageUrl = "https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },
                new() { RestaurantId = r4Id, Name = "Panna Cotta al Limoncello",   Description = "Silky vanilla panna cotta with a limoncello and Amalfi lemon curd, topped with candied lemon zest and fresh berries.",                              Price = 949m,   ImageUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },
                new() { RestaurantId = r4Id, Name = "San Pellegrino Spritz",       Description = "Aperol, Prosecco Superiore and San Pellegrino blood orange over ice. The quintessential Italian aperitivo.",                                         Price = 1149m,  ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=600", IsAvailable = true, DietType = "Veg",     Category = catDrinks },
            };

            // ════════════════════════════════════════════════════════════════════
            // RESTAURANT 5 — Rajput's Darbar  (Chandan Singh Rajput's restaurant)
            // Modern Royal Indian — elevated street food meets fine-dining
            // ════════════════════════════════════════════════════════════════════
            var r5Id = Guid.NewGuid();
            var chandanOwnerId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            var r5 = new Restaurant
            {
                Id             = r5Id,
                OwnerId        = chandanOwnerId,
                Name           = "Rajput's Darbar",
                Description    = "A regal dining experience inspired by the warrior kitchens of Rajputana. Chef Chandan Singh Rajput reimagines bold Rajasthani and Awadhi flavours with modern plating and premium ingredients — where every dish tells a story of heritage and pride.",
                Address        = "7 Rajmahal Road, Civil Lines, Jaipur 302006",
                CuisineTypes   = "Rajasthani, Awadhi, Modern Indian",
                LogoUrl        = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=400",
                ApprovalStatus = "Approved",
                IsOpen         = true,
                CreatedAt      = DateTime.UtcNow
            };

            var r5Menu = new List<MenuItem>
            {
                // Starters
                new() { RestaurantId = r5Id, Name = "Rajput Platter",               Description = "A royal sharing platter — crispy pyaaz kachori, mini dal baati, paneer tikka skewers and churma laddoo bites. The perfect introduction to Rajputana.",                Price = 1899m,  ImageUrl = "https://images.unsplash.com/photo-1567188040759-fb8a883dc6d8?w=600", IsAvailable = true, DietType = "Veg",     Category = catStarters },
                new() { RestaurantId = r5Id, Name = "Jungli Maas Croquettes",        Description = "Slow-cooked wild boar keema shaped into golden croquettes, spiced with juniper and black cardamom. Served with a smoked chilli aioli.",                               Price = 1599m,  ImageUrl = "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catStarters },
                new() { RestaurantId = r5Id, Name = "Ker Sangri Bruschetta",         Description = "Rajasthan's iconic desert beans and berries sautéed in mustard oil, served on charcoal-toasted sourdough with whipped goat cheese and pomegranate.",                  Price = 1249m,  ImageUrl = "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=600", IsAvailable = true, DietType = "Veg",     Category = catStarters },

                // Grills
                new() { RestaurantId = r5Id, Name = "Maharana's Lamb Chops",         Description = "New Zealand lamb chops marinated in a secret 14-spice Rajput masala and raw papaya for 24 hours. Charcoal-grilled and served with safed maas sauce.",                 Price = 3299m,  ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catGrills },
                new() { RestaurantId = r5Id, Name = "Tandoori Murgh Darbar",         Description = "Whole spring chicken marinated in hung curd, Kashmiri chilli and saffron. Slow-roasted in a clay tandoor for 45 minutes. Carved tableside.",                          Price = 2499m,  ImageUrl = "https://images.unsplash.com/photo-1567188040759-fb8a883dc6d8?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catGrills },
                new() { RestaurantId = r5Id, Name = "Paneer Shahi Seekh",            Description = "Hand-crumbled paneer mixed with cashew paste, green chilli and dried rose petals, shaped on skewers and grilled over charcoal. Served with saffron raita.",           Price = 1799m,  ImageUrl = "https://images.unsplash.com/photo-1631452180519-c014fe946bc7?w=600", IsAvailable = true, DietType = "Veg",     Category = catGrills },

                // Main Course
                new() { RestaurantId = r5Id, Name = "Safed Maas",                   Description = "The legendary white mutton curry of Rajputana — slow-cooked in a rich cashew and cream gravy with white pepper and cardamom. Mild, aromatic and deeply royal.",        Price = 2899m,  ImageUrl = "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },
                new() { RestaurantId = r5Id, Name = "Gatte ki Sabzi Royale",         Description = "Chickpea flour dumplings simmered in a tangy yoghurt-based gravy with fenugreek and dried mango. A Rajasthani classic elevated with truffle oil finish.",             Price = 1599m,  ImageUrl = "https://images.unsplash.com/photo-1546833999-b9f581a1996d?w=600", IsAvailable = true, DietType = "Veg",     Category = catMainCourse },
                new() { RestaurantId = r5Id, Name = "Awadhi Dum Gosht",              Description = "Bone-in mutton slow-cooked in a sealed handi with whole spices, caramelised onions and kewra water for 6 hours. The Nawabi way.",                                     Price = 3199m,  ImageUrl = "https://images.unsplash.com/photo-1603894584373-5ac82b2ae398?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catMainCourse },

                // Breads & Rice
                new() { RestaurantId = r5Id, Name = "Rajput Dum Biryani",            Description = "Aged Basmati rice layered with saffron-marinated mutton, fried onions and whole spices. Sealed with dough and slow-cooked on a tawa. Served with burani raita.",      Price = 2199m,  ImageUrl = "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600", IsAvailable = true, DietType = "Non-Veg", Category = catBreads },
                new() { RestaurantId = r5Id, Name = "Bajra Roti with Ghee",          Description = "Stone-ground pearl millet flatbread cooked on an iron tawa, served with a generous pour of cultured ghee and jaggery. A Rajasthani staple, done right.",              Price = 449m,   ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600", IsAvailable = true, DietType = "Veg",     Category = catBreads },

                // Desserts
                new() { RestaurantId = r5Id, Name = "Mawa Kachori Sundae",           Description = "Warm flaky mawa kachori filled with rabri, topped with vanilla bean ice cream, rose syrup and crushed pistachios. A Jodhpur classic reimagined.",                     Price = 1099m,  ImageUrl = "https://images.unsplash.com/photo-1551024506-0bccd828d307?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },
                new() { RestaurantId = r5Id, Name = "Ghevar Tart",                   Description = "Rajasthan's iconic honeycomb sweet reimagined as a French tart — crispy ghevar shell filled with saffron custard and topped with edible silver and rose petals.",       Price = 949m,   ImageUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600", IsAvailable = true, DietType = "Veg",     Category = catDesserts },

                // Drinks
                new() { RestaurantId = r5Id, Name = "Rajput Warrior Thandai",        Description = "A potent blend of almonds, saffron, black pepper, rose and cardamom in chilled full-fat milk. Served in a copper glass with a dried rose garnish.",                   Price = 699m,   ImageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=600", IsAvailable = true, DietType = "Veg",     Category = catDrinks },
                new() { RestaurantId = r5Id, Name = "Aam Panna Spritz",              Description = "Raw mango cooler with black salt, cumin and mint, topped with sparkling water. Refreshing, tangy and utterly Rajasthani.",                                             Price = 549m,   ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=600", IsAvailable = true, DietType = "Veg",     Category = catDrinks },
            };

            // ── Persist everything ───────────────────────────────────────────────
            r1.MenuItems = r1Menu;
            r2.MenuItems = r2Menu;
            r3.MenuItems = r3Menu;
            r4.MenuItems = r4Menu;
            r5.MenuItems = r5Menu;

            context.Restaurants.AddRange(r1, r2, r3, r4, r5);
            context.SaveChanges();
        }
    }
}
