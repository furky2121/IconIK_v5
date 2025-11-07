import ApiService from './api';

class VideoEgitimService {
    // Kategoriler
    async getKategoriler() {
        return ApiService.get('/VideoEgitim/kategoriler');
    }

    async saveKategori(kategori) {
        return ApiService.post('/VideoEgitim/kategori', kategori);
    }

    async deleteKategori(id) {
        return ApiService.delete(`/VideoEgitim/kategori/${id}`);
    }

    // Eğitimler
    async getTumEgitimler() {
        return ApiService.get('/VideoEgitim/tumegitimler');
    }

    async getEgitimlerByKategori(kategoriId) {
        return ApiService.get(`/VideoEgitim/kategori/${kategoriId}`);
    }

    async getBenimEgitimlerim() {
        return ApiService.get('/VideoEgitim/benim-egitimlerim');
    }

    async getEgitimDetay(id, personalId = null) {
        const url = personalId ? `/VideoEgitim/${id}?personelId=${personalId}` : `/VideoEgitim/${id}`;
        return ApiService.get(url);
    }

    async saveEgitim(egitim) {
        return ApiService.post('/VideoEgitim/egitim', egitim);
    }

    async searchEgitimler(searchTerm) {
        return ApiService.get(`/VideoEgitim/ara?q=${encodeURIComponent(searchTerm)}`);
    }

    // Atamalar
    async atamaYap(atama) {
        return ApiService.post('/VideoEgitim/atama', atama);
    }

    async topluAtamaYap(topluAtama) {
        return ApiService.post('/VideoEgitim/toplu-atama', topluAtama);
    }

    async getBekleyenEgitimler() {
        return ApiService.get('/VideoEgitim/bekleyen-egitimler');
    }

    // İzleme
    async izlemeKaydet(izlemeData) {
        return ApiService.post('/VideoEgitim/izleme', izlemeData);
    }

    // Progress güncelleme
    async updateProgress(progressData) {
        return ApiService.post('/VideoEgitim/update-progress', progressData);
    }

    // Video süresini API'dan çek
    async getVideoDuration(videoUrl) {
        return ApiService.post('/VideoEgitim/get-video-duration', { videoUrl });
    }

    // İstatistikler
    async getIstatistikler(personelId = null, departmanId = null) {
        let url = '/VideoEgitim/istatistikler';
        const params = [];
        
        if (personelId) params.push(`personelId=${personelId}`);
        if (departmanId) params.push(`departmanId=${departmanId}`);
        
        if (params.length > 0) {
            url += '?' + params.join('&');
        }
        
        return ApiService.get(url);
    }

    // Sertifika
    async sertifikaOlustur(videoEgitimId) {
        return ApiService.post(`/VideoEgitim/sertifika/${videoEgitimId}`);
    }

    // Raporlama - Yeni endpoint'ler
    async getIzlemeRaporu(personelId = null) {
        let url = '/VideoEgitim/izleme-raporu';
        if (personelId) {
            url += `?personelId=${personelId}`;
        }
        return ApiService.get(url);
    }

    async getIzlemeIstatistikleri(personelId = null) {
        let url = '/VideoEgitim/izleme-istatistikleri';
        if (personelId) {
            url += `?personelId=${personelId}`;
        }
        return ApiService.get(url);
    }

    // Yardımcı metodlar
    getVideoEmbedUrl(videoUrl) {
        // YouTube URL'sini embed formatına çevir
        if (videoUrl.includes('youtube.com/watch?v=')) {
            const videoId = videoUrl.split('v=')[1];
            const ampersandPosition = videoId.indexOf('&');
            if (ampersandPosition !== -1) {
                return `https://www.youtube.com/embed/${videoId.substring(0, ampersandPosition)}`;
            }
            return `https://www.youtube.com/embed/${videoId}`;
        }
        
        // Vimeo URL'sini embed formatına çevir
        if (videoUrl.includes('vimeo.com/')) {
            let videoId = null;
            
            // Grup linklerini parse et: https://vimeo.com/groups/114/videos/1017406920
            const groupMatch = videoUrl.match(/vimeo\.com\/groups\/\d+\/videos\/(\d+)/);
            if (groupMatch) {
                videoId = groupMatch[1];
            } else {
                // Normal Vimeo linklerini parse et: https://vimeo.com/1017406920
                const directMatch = videoUrl.match(/vimeo\.com\/(\d+)/);
                if (directMatch) {
                    videoId = directMatch[1];
                }
            }
            
            if (videoId) {
                return `https://player.vimeo.com/video/${videoId}`;
            }
        }
        
        // Diğer durumlar için orijinal URL'i döndür
        return videoUrl;
    }

    getThumbnailUrl(videoUrl) {
        if (!videoUrl) return null;

        // YouTube thumbnail (multiple URL formats)
        if (videoUrl.includes('youtube.com/watch?v=') || videoUrl.includes('youtu.be/')) {
            let videoId = null;
            
            if (videoUrl.includes('youtube.com/watch?v=')) {
                const urlParams = new URLSearchParams(new URL(videoUrl).search);
                videoId = urlParams.get('v');
            } else if (videoUrl.includes('youtu.be/')) {
                const match = videoUrl.match(/youtu\.be\/([^?&]+)/);
                videoId = match ? match[1] : null;
            }

            if (videoId) {
                // Try maxresdefault first, fallback to hqdefault if needed
                return `https://img.youtube.com/vi/${videoId}/maxresdefault.jpg`;
            }
        }
        
        // Vimeo thumbnail (requires API call for proper implementation)
        if (videoUrl.includes('vimeo.com/')) {
            let videoId = null;
            
            // Grup linklerini parse et: https://vimeo.com/groups/114/videos/1017406920
            const groupMatch = videoUrl.match(/vimeo\.com\/groups\/\d+\/videos\/(\d+)/);
            if (groupMatch) {
                videoId = groupMatch[1];
            } else {
                // Normal Vimeo linklerini parse et: https://vimeo.com/1017406920
                const directMatch = videoUrl.match(/vimeo\.com\/(\d+)/);
                if (directMatch) {
                    videoId = directMatch[1];
                }
            }
            
            if (videoId) {
                // Vimeo thumbnail servisi kullan
                return `https://vumbnail.com/${videoId}.jpg`;
            }
        }
        
        return null;
    }

    // Get fallback thumbnail URL
    getFallbackThumbnail() {
        return '/layout/images/icon_ik.png';
    }

    // Get thumbnail with fallback
    getThumbnailWithFallback(thumbnailUrl, videoUrl) {
        // If we have a thumbnail URL, use it
        if (thumbnailUrl && thumbnailUrl.trim() !== '') {
            return thumbnailUrl;
        }
        
        // Try to extract from video URL
        const extracted = this.getThumbnailUrl(videoUrl);
        if (extracted) return extracted;
        
        // Return fallback
        return this.getFallbackThumbnail();
    }

    // Validate video URL format
    isValidVideoUrl(url) {
        if (!url || typeof url !== 'string') return false;
        
        const videoUrlPatterns = [
            /^https?:\/\/(www\.)?(youtube\.com|youtu\.be)/,
            /^https?:\/\/(www\.)?vimeo\.com/,
            /^https?:\/\/.*\.(mp4|webm|ogg|avi|mov|wmv|flv)(\?.*)?$/i
        ];
        
        return videoUrlPatterns.some(pattern => pattern.test(url));
    }

    // Get video platform type
    getVideoPlatform(url) {
        if (!url) return 'unknown';
        
        if (url.includes('youtube.com') || url.includes('youtu.be')) {
            return 'youtube';
        }
        
        if (url.includes('vimeo.com')) {
            return 'vimeo';
        }
        
        if (/\.(mp4|webm|ogg|avi|mov|wmv|flv)(\?.*)?$/i.test(url)) {
            return 'direct';
        }
        
        return 'unknown';
    }

    formatDuration(minutes) {
        if (minutes < 60) {
            return `${minutes} dk`;
        }
        
        const hours = Math.floor(minutes / 60);
        const remainingMinutes = minutes % 60;
        
        if (remainingMinutes === 0) {
            return `${hours} saat`;
        }
        
        return `${hours} saat ${remainingMinutes} dk`;
    }

    getLevelBadgeClass(seviye) {
        switch (seviye?.toLowerCase()) {
            case 'başlangıç':
                return 'success';
            case 'orta':
                return 'warning';
            case 'ileri':
                return 'danger';
            default:
                return 'info';
        }
    }

    getProgressColor(percentage) {
        if (percentage < 25) return '#ef4444'; // Kırmızı
        if (percentage < 50) return '#f59e0b'; // Turuncu
        if (percentage < 75) return '#eab308'; // Sarı
        return '#22c55e'; // Yeşil
    }

    calculateProgress(izlemeYuzdesi, izlenmeMinimum) {
        return Math.min((izlemeYuzdesi / izlenmeMinimum) * 100, 100);
    }

    // Video player events için
    trackVideoProgress(videoEgitimId, currentTime, duration) {
        const percentage = Math.round((currentTime / duration) * 100);
        const progressData = {
            videoEgitimId: videoEgitimId,
            currentTime: Math.round(currentTime),
            duration: Math.round(duration),
            percentage: percentage,
            timestamp: new Date()
        };

        // Local storage'a progress kaydet
        const storageKey = `video_progress_${videoEgitimId}`;
        if (typeof window !== 'undefined') {
            localStorage.setItem(storageKey, JSON.stringify(progressData));
        }

        return progressData;
    }

    getStoredProgress(videoEgitimId) {
        const storageKey = `video_progress_${videoEgitimId}`;
        if (typeof window !== 'undefined') {
            const stored = localStorage.getItem(storageKey);
            return stored ? JSON.parse(stored) : null;
        }
        return null;
    }

    clearStoredProgress(videoEgitimId) {
        const storageKey = `video_progress_${videoEgitimId}`;
        if (typeof window !== 'undefined') {
            localStorage.removeItem(storageKey);
        }
    }

    // Atama işlemleri - Eğitim Katılımları için
    async getAtamalar() {
        return ApiService.get('/VideoEgitim/atamalar');
    }

    async createAtama(atama) {
        return ApiService.post('/VideoEgitim/atama', atama);
    }

    async updateAtama(id, atama) {
        return ApiService.put(`/VideoEgitim/atama/${id}`, atama);
    }

    async deleteAtama(id) {
        return ApiService.delete(`/VideoEgitim/atama/${id}`);
    }

    async getVideoEgitimler() {
        return this.getTumEgitimler();
    }

    // Sertifikalar
    async getSertifikalar() {
        return ApiService.get('/VideoEgitim/sertifikalar');
    }

    // Video eğitim istatistikleri
    async getVideoEgitimIstatistikleri() {
        return ApiService.get('/VideoEgitim/rapor-istatistikleri');
    }

    // Personel video eğitim özeti
    async getPersonelVideoEgitimOzeti() {
        return ApiService.get('/VideoEgitim/personel-egitim-ozeti');
    }

    // Departman raporu
    async getDepartmanRaporu(year = null) {
        const url = year ? `/VideoEgitim/departman-raporu?year=${year}` : '/VideoEgitim/departman-raporu';
        return ApiService.get(url);
    }

    // Bulk operations
    async getMultipleEgitimDetay(egitimIds) {
        const promises = egitimIds.map(id => this.getEgitimDetay(id));
        return Promise.all(promises);
    }

    // Notification helpers
    shouldShowReminder(atama) {
        if (!atama.VideoEgitim?.SonTamamlanmaTarihi) return false;
        
        const sonTarih = new Date(atama.VideoEgitim.SonTamamlanmaTarihi);
        const bugun = new Date();
        const fark = Math.ceil((sonTarih - bugun) / (1000 * 60 * 60 * 24));
        
        return fark <= 7 && fark > 0; // Son 7 gün
    }

    getReminderMessage(atama) {
        const sonTarih = new Date(atama.VideoEgitim.SonTamamlanmaTarihi);
        const bugun = new Date();
        const fark = Math.ceil((sonTarih - bugun) / (1000 * 60 * 60 * 24));
        
        if (fark === 1) {
            return 'Yarın son gün!';
        } else if (fark <= 3) {
            return `${fark} gün kaldı!`;
        } else {
            return `${fark} gün kaldı`;
        }
    }

    // Analytics helpers
    async getDetailedAnalytics(filters = {}) {
        const analytics = await this.getIstatistikler(filters.personelId, filters.departmanId);
        
        // Ek hesaplamalar
        if (analytics.success && analytics.data) {
            const data = analytics.data;
            
            // Tamamlanma oranı hesapla
            data.completionRate = data.toplamEgitim > 0 
                ? Math.round((data.tamamlananEgitim / data.toplamEgitim) * 100)
                : 0;
            
            // Ortalama izleme süresi (saat olarak)
            data.averageWatchTimeHours = data.toplamIzlemeSuresi 
                ? Math.round(data.toplamIzlemeSuresi / 60 * 100) / 100
                : 0;
                
            // Performance kategorisi
            if (data.completionRate >= 90) {
                data.performanceLevel = 'Mükemmel';
                data.performanceColor = '#22c55e';
            } else if (data.completionRate >= 70) {
                data.performanceLevel = 'İyi';
                data.performanceColor = '#eab308';
            } else if (data.completionRate >= 50) {
                data.performanceLevel = 'Orta';
                data.performanceColor = '#f59e0b';
            } else {
                data.performanceLevel = 'Geliştirilmeli';
                data.performanceColor = '#ef4444';
            }
        }
        
        return analytics;
    }

    // Aylık video eğitim trendi
    async getAylikEgitimTrendi(year = null) {
        const url = year ? `/VideoEgitim/aylik-egitim-trendi?year=${year}` : '/VideoEgitim/aylik-egitim-trendi';
        return ApiService.get(url);
    }
}

export default new VideoEgitimService();