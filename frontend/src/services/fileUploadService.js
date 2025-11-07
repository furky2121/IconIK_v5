import ApiService from './api';

class FileUploadService {
    async uploadAvatar(file) {
        const formData = new FormData();
        formData.append('file', file);

        const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace('/api', '') || 'http://localhost:5000';
        const response = await fetch(`${baseUrl}/api/fileupload/avatar`, {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || 'Dosya yüklenirken hata oluştu');
        }

        return data;
    }

    async deleteAvatar(fileName) {
        const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace('/api', '') || 'http://localhost:5000';
        const response = await fetch(`${baseUrl}/api/fileupload/avatar/${fileName}`, {
            method: 'DELETE'
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || 'Dosya silinirken hata oluştu');
        }

        return data;
    }

    getAvatarUrl(fileName) {
        if (!fileName) {
            return null;
        }
        // Eğer fileName tam URL ise direkt kullan
        if (fileName.startsWith('http://') || fileName.startsWith('https://')) {
            return fileName;
        }
        
        // Cache busting için session ve dosya adı bazlı hash
        const sessionId = sessionStorage.getItem('avatarRefresh') || Date.now();
        // Environment variable'dan base URL al
        const baseUrl = process.env.NEXT_PUBLIC_FILE_BASE_URL || 'http://localhost:5000';
        
        // Eğer fileName zaten /uploads/avatars/ ile başlıyorsa sadece base URL ekle
        if (fileName.startsWith('/uploads/avatars/')) {
            return `${baseUrl}${fileName}?v=${sessionId}`;
        }

        // Sadece dosya adı ise tam path oluştur
        return `${baseUrl}/uploads/avatars/${fileName}?v=${sessionId}`;
    }

    // Avatar cache'ini temizle
    refreshAvatarCache() {
        sessionStorage.setItem('avatarRefresh', Date.now());
        // Topbar ve diğer bileşenlere avatar yenilendiğini bildir
        window.dispatchEvent(new CustomEvent('avatarRefresh'));
    }
}

export default new FileUploadService();