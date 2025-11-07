using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Linq;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizasyonController : ControllerBase
    {
        private readonly IconIKContext _context;

        public OrganizasyonController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/Organizasyon/Sema
        [HttpGet("Sema")]
        public async Task<ActionResult<object>> GetOrganizasyonSemasi()
        {
            try
            {
                Console.WriteLine("=== GetOrganizasyonSemasi başlıyor ===");
                
                // Entity'leri direkt al, hiç transformation yapmadan
                var entities = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .Include(p => p.Yonetici)
                        .ThenInclude(y => y.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Where(p => p.Aktif)
                    .ToListAsync();
                    
                Console.WriteLine($"Entity'ler alındı: {entities.Count}");

                // Manuelde transformation yap
                var personeller = entities.Select(p => new PersonelHierarchy
                {
                    Id = p.Id,
                    Ad = p.Ad,
                    Soyad = p.Soyad,
                    Email = p.Email,
                    Telefon = p.Telefon,
                    FotografUrl = p.FotografUrl,
                    IseBaslamaTarihi = p.IseBaslamaTarihi,
                    YoneticiId = p.YoneticiId,
                    Pozisyon = p.Pozisyon != null ? new PozisyonInfo
                    {
                        Id = p.Pozisyon.Id,
                        Ad = p.Pozisyon.Ad,
                        Departman = p.Pozisyon.Departman != null ? new DepartmanInfo
                        {
                            Id = p.Pozisyon.Departman.Id,
                            Ad = p.Pozisyon.Departman.Ad,
                            Kod = p.Pozisyon.Departman.Kod
                        } : null,
                        Kademe = p.Pozisyon.Kademe != null ? new KademeInfo
                        {
                            Id = p.Pozisyon.Kademe.Id,
                            Ad = p.Pozisyon.Kademe.Ad,
                            Seviye = p.Pozisyon.Kademe.Seviye
                        } : null
                    } : null,
                    Yonetici = p.Yonetici != null ? new YoneticiInfo
                    {
                        Id = p.Yonetici.Id,
                        Ad = p.Yonetici.Ad,
                        Soyad = p.Yonetici.Soyad,
                        FotografUrl = p.Yonetici.FotografUrl,
                        PozisyonAd = p.Yonetici.Pozisyon != null ? p.Yonetici.Pozisyon.Ad : "Pozisyon Tanımsız",
                        DepartmanAd = p.Yonetici.Pozisyon != null && p.Yonetici.Pozisyon.Departman != null
                            ? p.Yonetici.Pozisyon.Departman.Ad : "Departman Tanımsız",
                        KademeSeviye = p.Yonetici.Pozisyon != null && p.Yonetici.Pozisyon.Kademe != null
                            ? p.Yonetici.Pozisyon.Kademe.Seviye : 9
                    } : null
                })
                .OrderBy(p => p.Pozisyon?.Kademe?.Seviye ?? 999)
                .ThenBy(p => p.Pozisyon?.Departman?.Ad ?? "")
                .ThenBy(p => p.Ad)
                .ToList();
                
                Console.WriteLine($"Transformation tamamlandı: {personeller.Count}");

                // Hiyerarşik yapı oluştur
                var organizasyonSemasi = BuildHierarchyTyped(personeller);

                // Departman bazında istatistikler
                var departmanIstatistikleri = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .Where(p => p.Aktif && p.Pozisyon != null && p.Pozisyon.Departman != null)
                    .GroupBy(p => new { 
                        DepartmanId = p.Pozisyon.Departman.Id, 
                        DepartmanAd = p.Pozisyon.Departman.Ad,
                        DepartmanKod = p.Pozisyon.Departman.Kod
                    })
                    .Select(g => new
                    {
                        departmanId = g.Key.DepartmanId,
                        departmanAd = g.Key.DepartmanAd,
                        departmanKod = g.Key.DepartmanKod,
                        toplamPersonel = g.Count(),
                        kademeDagilimi = g.GroupBy(p => new 
                        { 
                            KademeId = p.Pozisyon.Kademe.Id, 
                            KademeAd = p.Pozisyon.Kademe.Ad,
                            KademeSeviye = p.Pozisyon.Kademe.Seviye
                        })
                        .Select(kg => new
                        {
                            kademeId = kg.Key.KademeId,
                            kademeAd = kg.Key.KademeAd,
                            kademeSeviye = kg.Key.KademeSeviye,
                            personelSayisi = kg.Count()
                        })
                        .OrderBy(kg => kg.kademeSeviye)
                        .ToList()
                    })
                    .OrderBy(d => d.departmanAd)
                    .ToListAsync();

                var result = new
                {
                    organizasyonSemasi = organizasyonSemasi,
                    departmanIstatistikleri = departmanIstatistikleri,
                    toplamPersonel = personeller.Count,
                    toplamDepartman = departmanIstatistikleri.Count
                };

                return Ok(new { success = true, data = result, message = "Organizasyon şeması başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Organizasyon şeması hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Organizasyon şeması getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        private object BuildHierarchy(object personellerObj)
        {
            try
            {
                // List<T> generic tipini alabilmek için reflection kullan
                var listType = personellerObj.GetType();
                Console.WriteLine($"Input type: {listType.Name}");
                
                // IEnumerable olarak cast et ve dynamic list'e çevir
                var enumerable = personellerObj as System.Collections.IEnumerable;
                var personelList = new List<dynamic>();
                
                if (enumerable != null)
                {
                    foreach (var item in enumerable)
                    {
                        personelList.Add(item);
                    }
                }
                else
                {
                    Console.WriteLine("Personeller listesi null veya enumerable değil!");
                    return new List<object>();
                }
            Console.WriteLine($"\n=== BuildHierarchy başlıyor - Toplam personel: {personelList.Count} ===");
            
            // Hierarchy Node sınıfı tanımla
            var hierarchyNodes = new Dictionary<int, HierarchyNode>();
            
            // Tüm personelleri HierarchyNode'lara çevir
            foreach (dynamic personel in personelList)
            {
                var node = new HierarchyNode
                {
                    id = personel.id,
                    ad = personel.ad,
                    soyad = personel.soyad,
                    email = personel.email,
                    telefon = personel.telefon,
                    fotografUrl = personel.fotografUrl,
                    iseBaslamaTarihi = personel.iseBaslamaTarihi,
                    yoneticiId = personel.yoneticiId,
                    pozisyon = personel.pozisyon,
                    yonetici = personel.yonetici,
                    children = new List<HierarchyNode>()
                };
                
                hierarchyNodes[personel.id] = node;
                Console.WriteLine($"Node oluşturuldu: {personel.ad} {personel.soyad} (ID: {personel.id}, YöneticiId: {personel.yoneticiId})");
            }

            Console.WriteLine($"\n--- Parent-child ilişkileri kuruluyor ---");
            
            // Parent-child ilişkilerini kur
            foreach (var node in hierarchyNodes.Values.ToList())
            {
                if (node.yoneticiId.HasValue)
                {
                    Console.WriteLine($"Personel: {node.ad} {node.soyad} (ID: {node.id}) - Yönetici ID: {node.yoneticiId}");
                    
                    if (hierarchyNodes.ContainsKey(node.yoneticiId.Value))
                    {
                        var parent = hierarchyNodes[node.yoneticiId.Value];
                        parent.children.Add(node);
                        Console.WriteLine($"  -> Bağlandı: {node.ad} {node.soyad} -> {parent.ad} {parent.soyad}");
                        Console.WriteLine($"  -> Parent'in toplam child sayısı: {parent.children.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"  -> UYARI: Yönetici bulunamadı! (Yönetici ID: {node.yoneticiId})");
                    }
                }
                else
                {
                    Console.WriteLine($"Root personel: {node.ad} {node.soyad} (ID: {node.id})");
                }
            }

            // Root node'ları bul
            var rootNodes = hierarchyNodes.Values
                .Where(n => !n.yoneticiId.HasValue)
                .ToList();
            
            Console.WriteLine($"\n--- Sonuçlar ---");
            Console.WriteLine($"Root node sayısı: {rootNodes.Count}");
            
            foreach(var root in rootNodes)
            {
                Console.WriteLine($"Root: {root.ad} {root.soyad} - Children: {root.children.Count}");
                PrintHierarchy(root, 1);
            }
            
            if (!rootNodes.Any())
            {
                Console.WriteLine("Root node bulunamadı, en yüksek kademeyi root yapıyoruz...");
                
                // En düşük kademe seviyesine sahip personeli root yap
                var enYuksekKademe = hierarchyNodes.Values
                    .Where(n => n.pozisyon != null && ((dynamic)n.pozisyon).kademe != null)
                    .OrderBy(n => (int)((dynamic)n.pozisyon).kademe.seviye)
                    .FirstOrDefault();
                    
                if (enYuksekKademe != null)
                {
                    Console.WriteLine($"En yüksek kademe seçildi: {enYuksekKademe.ad} {enYuksekKademe.soyad}");
                    rootNodes.Add(enYuksekKademe);
                }
                else
                {
                    // Hiçbiri yoksa ilk personeli root yap
                    rootNodes.Add(hierarchyNodes.Values.First());
                }
            }

                Console.WriteLine($"=== BuildHierarchy tamamlandı ===\n");

                // HierarchyNode'ları anonymous object'lere çevir
                return rootNodes.Select(ConvertToAnonymousObject).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BuildHierarchy hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<object>();
            }
        }
        
        private object BuildHierarchyTyped(List<PersonelHierarchy> personeller)
        {
            Console.WriteLine($"\n=== BuildHierarchyTyped başlıyor - Toplam personel: {personeller.Count} ===");
            
            var hierarchyNodes = new Dictionary<int, HierarchyNodeTyped>();
            
            // Tüm personelleri HierarchyNodeTyped'a çevir
            foreach (var personel in personeller)
            {
                var node = new HierarchyNodeTyped
                {
                    Id = personel.Id,
                    Ad = personel.Ad,
                    Soyad = personel.Soyad,
                    Email = personel.Email,
                    Telefon = personel.Telefon,
                    FotografUrl = personel.FotografUrl,
                    IseBaslamaTarihi = personel.IseBaslamaTarihi,
                    YoneticiId = personel.YoneticiId,
                    Pozisyon = personel.Pozisyon,
                    Yonetici = personel.Yonetici,
                    Children = new List<HierarchyNodeTyped>()
                };
                
                hierarchyNodes[personel.Id] = node;
                Console.WriteLine($"Node oluşturuldu: {personel.Ad} {personel.Soyad} (ID: {personel.Id}, YöneticiId: {personel.YoneticiId})");
            }

            Console.WriteLine($"\n--- Parent-child ilişkileri kuruluyor ---");
            
            // Parent-child ilişkilerini kur
            foreach (var node in hierarchyNodes.Values.ToList())
            {
                if (node.YoneticiId.HasValue)
                {
                    Console.WriteLine($"Personel: {node.Ad} {node.Soyad} (ID: {node.Id}) - Yönetici ID: {node.YoneticiId}");
                    
                    if (hierarchyNodes.ContainsKey(node.YoneticiId.Value))
                    {
                        var parent = hierarchyNodes[node.YoneticiId.Value];
                        parent.Children.Add(node);
                        Console.WriteLine($"  -> Bağlandı: {node.Ad} {node.Soyad} -> {parent.Ad} {parent.Soyad}");
                        Console.WriteLine($"  -> Parent'in toplam child sayısı: {parent.Children.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"  -> UYARI: Yönetici bulunamadı! (Yönetici ID: {node.YoneticiId})");
                    }
                }
                else
                {
                    Console.WriteLine($"Root personel: {node.Ad} {node.Soyad} (ID: {node.Id})");
                }
            }

            // Root node'ları bul
            var rootNodes = hierarchyNodes.Values
                .Where(n => !n.YoneticiId.HasValue)
                .ToList();
            
            Console.WriteLine($"\n--- Sonuçlar ---");
            Console.WriteLine($"Root node sayısı: {rootNodes.Count}");
            
            foreach(var root in rootNodes)
            {
                Console.WriteLine($"Root: {root.Ad} {root.Soyad} - Children: {root.Children.Count}");
                PrintHierarchyTyped(root, 1);
            }
            
            if (!rootNodes.Any())
            {
                Console.WriteLine("Root node bulunamadı, en yüksek kademeyi root yapıyoruz...");
                
                var enYuksekKademe = hierarchyNodes.Values
                    .Where(n => n.Pozisyon?.Kademe != null)
                    .OrderBy(n => n.Pozisyon.Kademe.Seviye)
                    .FirstOrDefault();
                    
                if (enYuksekKademe != null)
                {
                    Console.WriteLine($"En yüksek kademe seçildi: {enYuksekKademe.Ad} {enYuksekKademe.Soyad}");
                    rootNodes.Add(enYuksekKademe);
                }
                else
                {
                    rootNodes.Add(hierarchyNodes.Values.First());
                }
            }

            Console.WriteLine($"=== BuildHierarchyTyped tamamlandı ===\n");

            return rootNodes.Select(ConvertTypedToAnonymousObject).ToList();
        }
        
        private void PrintHierarchyTyped(HierarchyNodeTyped node, int level)
        {
            var indent = new string(' ', level * 2);
            Console.WriteLine($"{indent}- {node.Ad} {node.Soyad} (Children: {node.Children.Count})");
            foreach(var child in node.Children)
            {
                PrintHierarchyTyped(child, level + 1);
            }
        }
        
        private void PrintHierarchy(HierarchyNode node, int level)
        {
            var indent = new string(' ', level * 2);
            Console.WriteLine($"{indent}- {node.ad} {node.soyad} (Children: {node.children.Count})");
            foreach(var child in node.children)
            {
                PrintHierarchy(child, level + 1);
            }
        }
        
        private object ConvertTypedToAnonymousObject(HierarchyNodeTyped node)
        {
            return new
            {
                id = node.Id,
                ad = node.Ad,
                soyad = node.Soyad,
                email = node.Email,
                telefon = node.Telefon,
                fotografUrl = node.FotografUrl,
                iseBaslamaTarihi = node.IseBaslamaTarihi,
                yoneticiId = node.YoneticiId,
                pozisyon = node.Pozisyon != null ? new
                {
                    id = node.Pozisyon.Id,
                    ad = node.Pozisyon.Ad,
                    departman = node.Pozisyon.Departman != null ? new
                    {
                        id = node.Pozisyon.Departman.Id,
                        ad = node.Pozisyon.Departman.Ad,
                        kod = node.Pozisyon.Departman.Kod
                    } : null,
                    kademe = node.Pozisyon.Kademe != null ? new
                    {
                        id = node.Pozisyon.Kademe.Id,
                        ad = node.Pozisyon.Kademe.Ad,
                        seviye = node.Pozisyon.Kademe.Seviye
                    } : null
                } : null,
                yonetici = node.Yonetici != null ? new
                {
                    id = node.Yonetici.Id,
                    ad = node.Yonetici.Ad,
                    soyad = node.Yonetici.Soyad,
                    fotografUrl = node.Yonetici.FotografUrl,
                    pozisyonAd = node.Yonetici.PozisyonAd,
                    departmanAd = node.Yonetici.DepartmanAd,
                    kademeSeviye = node.Yonetici.KademeSeviye
                } : null,
                children = node.Children.Select(ConvertTypedToAnonymousObject).ToList()
            };
        }
        
        private object ConvertToAnonymousObject(HierarchyNode node)
        {
            return new
            {
                id = node.id,
                ad = node.ad,
                soyad = node.soyad,
                email = node.email,
                telefon = node.telefon,
                fotografUrl = node.fotografUrl,
                iseBaslamaTarihi = node.iseBaslamaTarihi,
                yoneticiId = node.yoneticiId,
                pozisyon = node.pozisyon,
                yonetici = node.yonetici,
                children = node.children.Select(ConvertToAnonymousObject).ToList()
            };
        }
        
        // Model sınıfları
        private class PersonelHierarchy
        {
            public int Id { get; set; }
            public string Ad { get; set; }
            public string Soyad { get; set; }
            public string Email { get; set; }
            public string Telefon { get; set; }
            public string FotografUrl { get; set; }
            public DateTime? IseBaslamaTarihi { get; set; }
            public int? YoneticiId { get; set; }
            public PozisyonInfo Pozisyon { get; set; }
            public YoneticiInfo Yonetici { get; set; }
        }

        private class PozisyonInfo
        {
            public int Id { get; set; }
            public string Ad { get; set; }
            public DepartmanInfo Departman { get; set; }
            public KademeInfo Kademe { get; set; }
        }

        private class DepartmanInfo
        {
            public int Id { get; set; }
            public string Ad { get; set; }
            public string Kod { get; set; }
        }

        private class KademeInfo
        {
            public int Id { get; set; }
            public string Ad { get; set; }
            public int Seviye { get; set; }
        }

        private class YoneticiInfo
        {
            public int Id { get; set; }
            public string Ad { get; set; }
            public string Soyad { get; set; }
            public string FotografUrl { get; set; }
            public string PozisyonAd { get; set; }
            public string DepartmanAd { get; set; }
            public int KademeSeviye { get; set; }
        }
        
        private class HierarchyNodeTyped
        {
            public int Id { get; set; }
            public string Ad { get; set; }
            public string Soyad { get; set; }
            public string Email { get; set; }
            public string Telefon { get; set; }
            public string FotografUrl { get; set; }
            public DateTime? IseBaslamaTarihi { get; set; }
            public int? YoneticiId { get; set; }
            public PozisyonInfo Pozisyon { get; set; }
            public YoneticiInfo Yonetici { get; set; }
            public List<HierarchyNodeTyped> Children { get; set; }
        }
        
        private class HierarchyNode
        {
            public int id { get; set; }
            public string ad { get; set; }
            public string soyad { get; set; }
            public string email { get; set; }
            public string telefon { get; set; }
            public string fotografUrl { get; set; }
            public DateTime? iseBaslamaTarihi { get; set; }
            public int? yoneticiId { get; set; }
            public dynamic pozisyon { get; set; }
            public dynamic yonetici { get; set; }
            public List<HierarchyNode> children { get; set; }
        }
    }
}