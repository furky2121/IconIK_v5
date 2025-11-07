const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000/api';

class ApiService {
    getBaseURL() {
        return API_BASE_URL;
    }

    async request(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        const config = {
            ...options,
        };

        // JWT Token ekle
        const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
        
        // Only set Content-Type for JSON if not FormData
        if (!(options.body instanceof FormData)) {
            config.headers = {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` }),
                ...options.headers,
            };
        } else {
            config.headers = {
                ...(token && { 'Authorization': `Bearer ${token}` }),
                ...options.headers,
            };
        }

        try {
            const response = await fetch(url, config);

            // Check if response has content to parse
            let data = null;
            const contentType = response.headers.get('content-type');

            if (contentType && contentType.includes('application/json') && response.status !== 204) {
                data = await response.json();
            } else if (response.status === 204) {
                // No Content - successful delete/update with no response body
                data = { success: true, message: 'İşlem başarılı' };
            } else {
                // Try to get text content for error cases
                try {
                    const text = await response.text();
                    if (text) {
                        data = { message: text };
                    } else {
                        data = { message: 'No response data' };
                    }
                } catch {
                    data = { message: 'No response data' };
                }
            }

            // Başarılı status kodları: 200-299 aralığı
            if (!response.ok) {
                // Provide more specific error messages based on status code
                let errorMessage = data?.message || data?.title;
                if (!errorMessage) {
                    switch (response.status) {
                        case 400:
                            errorMessage = 'Geçersiz istek.';
                            break;
                        case 401:
                            errorMessage = 'Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.';
                            break;
                        case 403:
                            errorMessage = 'Bu işlem için yetkiniz bulunmuyor.';
                            break;
                        case 404:
                            errorMessage = 'İstenen kaynak bulunamadı.';
                            break;
                        case 415:
                            errorMessage = 'Desteklenmeyen dosya formatı.';
                            break;
                        case 500:
                            errorMessage = 'Sunucu hatası oluştu. Lütfen tekrar deneyin.';
                            break;
                        case 502:
                        case 503:
                        case 504:
                            errorMessage = 'Sunucu şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.';
                            break;
                        default:
                            errorMessage = `HTTP ${response.status}: ${response.statusText}`;
                    }
                }

                // Create error with response data attached
                const error = new Error(errorMessage || 'Bir hata oluştu');
                error.response = { data, status: response.status };
                throw error;
            }

            return data;
        } catch (error) {
            throw error;
        }
    }

    // GET request
    async get(endpoint) {
        return this.request(endpoint, { method: 'GET' });
    }

    // POST request
    async post(endpoint, data) {
        // FormData için özel işlem - JSON.stringify kullanma
        if (data instanceof FormData) {
            return this.request(endpoint, {
                method: 'POST',
                body: data
            });
        }

        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(data),
        });
    }

    // PUT request
    async put(endpoint, data) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data),
        });
    }

    // DELETE request
    async delete(endpoint) {
        return this.request(endpoint, { method: 'DELETE' });
    }

    // PATCH request
    async patch(endpoint, data = null) {
        const options = { method: 'PATCH' };
        if (data) {
            options.body = JSON.stringify(data);
        }
        return this.request(endpoint, options);
    }

    // POST request for FormData (file uploads)
    async postFormData(endpoint, formData) {
        return this.request(endpoint, {
            method: 'POST',
            body: formData,
            headers: {
                // Don't set Content-Type for FormData, let browser set it
            }
        });
    }
}

export { ApiService };
export default new ApiService();