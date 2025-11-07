import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Card } from 'primereact/card';
import { DataView, DataViewLayoutOptions } from 'primereact/dataview';
import { Button } from 'primereact/button';
import { ProgressBar } from 'primereact/progressbar';
import { Badge } from 'primereact/badge';
import { Toast } from 'primereact/toast';
import { Skeleton } from 'primereact/skeleton';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { Toolbar } from 'primereact/toolbar';
import { Tag } from 'primereact/tag';
import { Image } from 'primereact/image';
import videoEgitimService from '../services/videoEgitimService';
import './BanaAtananEgitimler.css';

const BanaAtananEgitimler = () => {
    const router = useRouter();
    const toast = useRef(null);
    const [loading, setLoading] = useState(true);
    const [egitimler, setEgitimler] = useState([]);
    const [filteredEgitimler, setFilteredEgitimler] = useState([]);
    
    // Filter states
    const [globalFilter, setGlobalFilter] = useState('');
    const [durumFilter, setDurumFilter] = useState(null);
    const [layout, setLayout] = useState('grid');
    
    const durumOptions = [
        { label: 'Tümü', value: null },
        { label: 'Yeni', value: 'yeni' },
        { label: 'Devam Ediyor', value: 'devam' },
        { label: 'Tamamlandı', value: 'tamamlandi' }
    ];

    useEffect(() => {
        // İlk yüklendiğinde token kontrolü yap
        if (typeof window !== 'undefined') {
            const token = localStorage.getItem('token');
            if (!token) {
                router.push('/auth/login');
                return;
            }
            loadEgitimler();
        }
    }, []);

    useEffect(() => {
        filterEgitimler();
    }, [egitimler, globalFilter, durumFilter]);

    const loadEgitimler = async () => {
        setLoading(true);
        try {
            let token = null; // Token'ı dış scope'ta tanımla
            
            // Token kontrolü
            if (typeof window !== 'undefined') {
                token = localStorage.getItem('token');
                if (!token) {
                    router.push('/auth/login');
                    return;
                }
            } else {
                return; // SSR sırasında hiçbir şey yapma
            }

            // Debug: Token içeriğini kontrol et
            if (token) { // Token varsa kontrol et
                try {
                    const payload = JSON.parse(atob(token.split('.')[1]));
            // console.log('Token payload:', payload);
            // console.log('Token expiry:', new Date(payload.exp * 1000));
            // console.log('Current time:', new Date());
                } catch (e) {
            // console.error('Token parse error:', e);
                }
            }

            const response = await videoEgitimService.getBenimEgitimlerim();
            if (response.success) {
                setEgitimler(response.data);
            } else {
                toast.current?.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: response.message || 'Eğitimler yüklenirken hata oluştu.'
                });
            }
        } catch (error) {
            // console.error('Error loading trainings:', error);
            
            // 401 hatası durumunda login sayfasına yönlendir
            if (error.message?.includes('Kullanıcı bilgisi bulunamadı') || 
                error.message?.includes('401') ||
                error.message?.includes('Unauthorized')) {
                if (typeof window !== 'undefined') {
                    localStorage.removeItem('token');
                }
                router.push('/auth/login');
                return;
            }
            
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Eğitimler yüklenirken hata oluştu.'
            });
        } finally {
            setLoading(false);
        }
    };

    const filterEgitimler = () => {
        let filtered = [...egitimler];

        // Global filter (arama)
        if (globalFilter) {
            filtered = filtered.filter(egitim => 
                (egitim.Baslik || egitim.baslik)?.toLowerCase().includes(globalFilter.toLowerCase()) ||
                (egitim.Aciklama || egitim.aciklama)?.toLowerCase().includes(globalFilter.toLowerCase()) ||
                (egitim.KategoriAd || egitim.kategoriAd)?.toLowerCase().includes(globalFilter.toLowerCase()) ||
                (egitim.Egitmen || egitim.egitmen)?.toLowerCase().includes(globalFilter.toLowerCase())
            );
        }

        // Durum filter
        if (durumFilter) {
            filtered = filtered.filter(egitim => {
                const durum = getEgitimDurumu(egitim);
                return durum.value === durumFilter;
            });
        }

        setFilteredEgitimler(filtered);
    };

    const getEgitimDurumu = (egitim) => {
        // Öncelikle database'den gelen tamamlanma durumunu kontrol et
        const tamamlandiMi = egitim.TamamlandiMi || egitim.tamamlandiMi || false;
        const izlemeYuzdesi = egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi || 0;
        const izlenmeMinimum = egitim.IzlenmeMinimum || egitim.izlenmeMinimum || 80;

        // DB'de tamamlandı olarak işaretlenmişse veya minimum orana ulaşmışsa
        if (tamamlandiMi || izlemeYuzdesi >= izlenmeMinimum) {
            return { label: 'Tamamlandı', value: 'tamamlandi', severity: 'success' };
        } else if (izlemeYuzdesi > 0) {
            return { label: 'Devam Ediyor', value: 'devam', severity: 'warning' };
        } else {
            return { label: 'Yeni', value: 'yeni', severity: 'info' };
        }
    };

    const formatSure = (dakika) => {
        const saat = Math.floor(dakika / 60);
        const kalanDakika = dakika % 60;
        return saat > 0 ? `${saat}sa ${kalanDakika}dk` : `${kalanDakika}dk`;
    };

    const handleEgitimIzle = (egitim) => {
        // Farklı field name'leri deneyip ID'yi bulalım
        const videoId = egitim.Id || egitim.id || egitim.VideoEgitimId || egitim.videoEgitimId;
        
        if (videoId) {
            router.push(`/egitimler/izle/${videoId}`);
        } else {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Video ID bulunamadı.'
            });
        }
    };

    const renderGridItem = (egitim) => {
        const durum = getEgitimDurumu(egitim);
        
        return (
            <div className="col-12 sm:col-6 lg:col-12 xl:col-4 p-2" key={egitim.Id || egitim.id}>
                <Card className="h-full training-card">
                    <div className="flex flex-column h-full">
                        {/* Video Thumbnail */}
                        <div className="relative mb-3">
                            <div 
                                className="relative cursor-pointer video-thumbnail-container"
                                onClick={() => handleEgitimIzle(egitim)}
                                style={{ overflow: 'hidden' }}
                                title={`${egitim.Baslik} - Videoyu izlemek için tıklayın`}
                            >
                                <Image 
                                    src={videoEgitimService.getThumbnailWithFallback(egitim.ThumbnailUrl || egitim.thumbnailUrl, egitim.VideoUrl || egitim.videoUrl)} 
                                    alt={egitim.Baslik || egitim.baslik}
                                    width="100%"
                                    height="200"
                                    className="border-round"
                                    preview={false}
                                    style={{ 
                                        objectFit: 'cover',
                                        transition: 'transform 0.3s ease'
                                    }}
                                    onMouseEnter={(e) => e.target.style.transform = 'scale(1.05)'}
                                    onMouseLeave={(e) => e.target.style.transform = 'scale(1)'}
                                />
                                {/* Play Button or Completion Overlay */}
                                {(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi || 0) >= (egitim.IzlenmeMinimum || egitim.izlenmeMinimum || 80) ? (
                                    /* Completed Overlay */
                                    <div className="absolute inset-0 bg-black-alpha-70 flex align-items-center justify-content-center border-round" 
                                         style={{ cursor: 'pointer' }}
                                    >
                                        <div className="text-center text-white">
                                            <i className="pi pi-check-circle text-4xl mb-2 text-green-400"></i>
                                            <div className="text-sm font-medium">Eğitim Tamamlandı</div>
                                            <div className="text-xs mt-1 opacity-80">Tekrar izlemek için tıklayın</div>
                                        </div>
                                    </div>
                                ) : (
                                    /* Play Button Overlay */
                                    <div className="video-play-overlay" 
                                         style={{ 
                                             backgroundColor: 'rgba(0, 0, 0, 0.7)',
                                             borderRadius: '50%',
                                             width: '60px',
                                             height: '60px',
                                             display: 'flex',
                                             alignItems: 'center',
                                             justifyContent: 'center',
                                             cursor: 'pointer'
                                         }}
                                    >
                                        <i className="pi pi-play text-white text-2xl" style={{ marginLeft: '4px' }}></i>
                                    </div>
                                )}
                                {/* Duration Badge */}
                                {egitim.Sure && (
                                    <div className="absolute bottom-0 right-0 p-2">
                                        <span className="bg-black-alpha-70 text-white px-2 py-1 border-round text-sm duration-badge">
                                            {formatSure(egitim.Sure || egitim.sure)}
                                        </span>
                                    </div>
                                )}
                                
                                {/* Progress indicator on thumbnail for partially watched videos */}
                                {(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi) > 0 && (
                                    <div className="progress-indicator" style={{ 
                                        position: 'absolute',
                                        bottom: '0',
                                        left: '0',
                                        right: '0',
                                        height: '4px',
                                        backgroundColor: 'rgba(255, 255, 255, 0.3)',
                                        borderRadius: '0 0 8px 8px',
                                        zIndex: 2
                                    }}>
                                        <div className="progress-fill" style={{
                                            width: `${Math.floor(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi)}%`,
                                            height: '100%',
                                            background: 'linear-gradient(90deg, #4CAF50, #8BC34A)',
                                            borderRadius: '0 0 8px 8px'
                                        }} />
                                    </div>
                                )}
                            </div>
                            <div className="absolute top-0 right-0 p-2">
                                <Tag value={durum.label} severity={durum.severity} />
                            </div>
                            {(egitim.ZorunluMu || egitim.zorunluMu) && (
                                <div className="absolute top-0 left-0 p-2">
                                    <Tag value="Zorunlu" severity="danger" />
                                </div>
                            )}
                        </div>

                        {/* Eğitim Bilgileri */}
                        <div className="flex-1">
                            <h5 className="mt-0 mb-2 text-900 font-medium">{egitim.Baslik || egitim.baslik}</h5>
                            
                            <div className="mb-2">
                                <small className="text-600">
                                    <i className="pi pi-bookmark mr-1"></i>
                                    {egitim.KategoriAd || egitim.kategoriAd || 'Kategori Belirtilmemiş'}
                                </small>
                            </div>

                            <div className="mb-2">
                                <small className="text-600">
                                    <i className="pi pi-clock mr-1"></i>
                                    {formatSure(egitim.Sure || egitim.sure)}
                                </small>
                            </div>

                            {(egitim.Egitmen || egitim.egitmen) && (
                                <div className="mb-2">
                                    <small className="text-600">
                                        <i className="pi pi-user mr-1"></i>
                                        {egitim.Egitmen || egitim.egitmen || 'Eğitmen Belirtilmemiş'}
                                    </small>
                                </div>
                            )}

                            {(egitim.Aciklama || egitim.aciklama) && (
                                <p className="text-600 line-height-3 mb-3 text-sm">
                                    {(egitim.Aciklama || egitim.aciklama).length > 100 ? 
                                        (egitim.Aciklama || egitim.aciklama).substring(0, 100) + '...' : 
                                        (egitim.Aciklama || egitim.aciklama)
                                    }
                                </p>
                            )}

                            {/* İlerleme Çubuğu */}
                            {(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi) > 0 && (
                                <div className="mb-3">
                                    <div className="flex justify-content-between align-items-center mb-2">
                                        <small className="text-600 font-medium">İlerleme</small>
                                        <small className="text-700 font-semibold">{Math.floor(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi)}%</small>
                                    </div>
                                    <div className="card-progress-bar">
                                        <div 
                                            className="card-progress-fill"
                                            style={{ width: `${Math.floor(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi)}%` }}
                                        />
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Aksiyon Butonları */}
                        <div className="mt-auto">
                            <Button 
                                label={(egitim.TamamlandiMi || egitim.tamamlandiMi) ? 'Tekrar İzle' : (egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi) > 0 ? 'Devam Et' : 'İzle'}
                                icon="pi pi-play"
                                className="w-full"
                                onClick={() => handleEgitimIzle(egitim)}
                            />
                        </div>
                    </div>
                </Card>
            </div>
        );
    };

    const renderListItem = (egitim) => {
        const durum = getEgitimDurumu(egitim);
        
        return (
            <div className="col-12 p-2" key={egitim.Id || egitim.id}>
                <Card>
                    <div className="flex align-items-center">
                        {/* Video Thumbnail */}
                        <div className="mr-3 relative">
                            <div 
                                className="relative cursor-pointer video-thumbnail-container"
                                onClick={() => handleEgitimIzle(egitim)}
                                style={{ overflow: 'hidden' }}
                                title={`${egitim.Baslik} - Videoyu izlemek için tıklayın`}
                            >
                                <Image 
                                    src={videoEgitimService.getThumbnailWithFallback(egitim.ThumbnailUrl || egitim.thumbnailUrl, egitim.VideoUrl || egitim.videoUrl)} 
                                    alt={egitim.Baslik || egitim.baslik}
                                    width="120"
                                    height="80"
                                    className="border-round"
                                    preview={false}
                                    style={{ 
                                        objectFit: 'cover',
                                        transition: 'transform 0.3s ease'
                                    }}
                                    onMouseEnter={(e) => e.target.style.transform = 'scale(1.1)'}
                                    onMouseLeave={(e) => e.target.style.transform = 'scale(1)'}
                                />
                                {/* Play Button Overlay */}
                                <div className="video-play-overlay" 
                                     style={{ 
                                         backgroundColor: 'rgba(0, 0, 0, 0.7)',
                                         borderRadius: '50%',
                                         width: '30px',
                                         height: '30px',
                                         display: 'flex',
                                         alignItems: 'center',
                                         justifyContent: 'center',
                                         cursor: 'pointer'
                                     }}
                                >
                                    <i className="pi pi-play text-white text-sm" style={{ marginLeft: '2px' }}></i>
                                </div>
                                {/* Duration Badge */}
                                {egitim.Sure && (
                                    <div className="absolute bottom-0 right-0" style={{ margin: '4px' }}>
                                        <span className="bg-black-alpha-70 text-white px-1 border-round text-xs duration-badge">
                                            {formatSure(egitim.Sure || egitim.sure)}
                                        </span>
                                    </div>
                                )}
                                
                                {/* Progress indicator on thumbnail */}
                                {(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi) > 0 && (
                                    <div className="progress-indicator" style={{ 
                                        position: 'absolute',
                                        bottom: '0',
                                        left: '0',
                                        right: '0',
                                        height: '3px',
                                        backgroundColor: 'rgba(255, 255, 255, 0.3)',
                                        borderRadius: '0 0 8px 8px',
                                        zIndex: 2
                                    }}>
                                        <div className="progress-fill" style={{
                                            width: `${egitim.IzlemeYuzdesi}%`,
                                            height: '100%',
                                            background: 'linear-gradient(90deg, #4CAF50, #8BC34A)',
                                            borderRadius: '0 0 8px 8px'
                                        }} />
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Eğitim Bilgileri */}
                        <div className="flex-1 mr-3">
                            <div className="flex align-items-center mb-2">
                                <h5 className="mt-0 mb-0 mr-2 text-900">{egitim.Baslik || egitim.baslik}</h5>
                                <Tag value={durum.label} severity={durum.severity} className="mr-2" />
                                {(egitim.ZorunluMu || egitim.zorunluMu) && <Tag value="Zorunlu" severity="danger" />}
                            </div>
                            
                            <div className="flex align-items-center mb-2 text-600">
                                <i className="pi pi-bookmark mr-1"></i>
                                <span className="mr-3">{egitim.KategoriAd || egitim.kategoriAd || 'Kategori Belirtilmemiş'}</span>
                                <i className="pi pi-clock mr-1"></i>
                                <span className="mr-3">{formatSure(egitim.Sure)}</span>
                                {(egitim.Egitmen || egitim.egitmen) && (
                                    <>
                                        <i className="pi pi-user mr-1"></i>
                                        <span>{egitim.Egitmen || egitim.egitmen}</span>
                                    </>
                                )}
                            </div>

                            {(egitim.Aciklama || egitim.aciklama) && (
                                <p className="text-600 line-height-3 mb-2">
                                    {(egitim.Aciklama || egitim.aciklama).length > 150 ? 
                                        (egitim.Aciklama || egitim.aciklama).substring(0, 150) + '...' : 
                                        (egitim.Aciklama || egitim.aciklama)
                                    }
                                </p>
                            )}

                            {/* İlerleme Çubuğu */}
                            {(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi) > 0 && (
                                <div className="mt-2">
                                    <div className="flex justify-content-between align-items-center mb-1">
                                        <small className="text-600 font-medium">İlerleme</small>
                                        <small className="text-700 font-semibold">{Math.floor(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi)}%</small>
                                    </div>
                                    <div className="card-progress-bar">
                                        <div 
                                            className="card-progress-fill"
                                            style={{ width: `${Math.floor(egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi)}%` }}
                                        />
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Aksiyon Butonları */}
                        <div>
                            <Button 
                                label={(egitim.TamamlandiMi || egitim.tamamlandiMi) ? 'Tekrar İzle' : (egitim.IzlemeYuzdesi || egitim.izlemeYuzdesi) > 0 ? 'Devam Et' : 'İzle'}
                                icon="pi pi-play"
                                onClick={() => handleEgitimIzle(egitim)}
                            />
                        </div>
                    </div>
                </Card>
            </div>
        );
    };

    const renderEmpty = () => {
        if (loading) return null;
        
        return (
            <div className="col-12">
                <Card>
                    <div className="text-center py-5">
                        <i className="pi pi-inbox text-6xl text-400 mb-3"></i>
                        <h4 className="text-500 mb-2">Size atanmış eğitim bulunamadı</h4>
                        <p className="text-600">
                            {globalFilter || durumFilter ? 
                                'Filtreleme kriterlerinize uygun eğitim bulunamadı.' :
                                'Henüz size atanmış bir video eğitimi bulunmamaktadır.'
                            }
                        </p>
                    </div>
                </Card>
            </div>
        );
    };

    const renderSkeleton = () => {
        const skeletonItems = Array.from({ length: 6 }, (_, i) => (
            <div className="col-12 sm:col-6 lg:col-12 xl:col-4 p-2" key={i}>
                <Card>
                    <Skeleton width="100%" height="200px" className="mb-3" />
                    <Skeleton width="80%" height="1.5rem" className="mb-2" />
                    <Skeleton width="60%" height="1rem" className="mb-2" />
                    <Skeleton width="40%" height="1rem" className="mb-3" />
                    <Skeleton width="100%" height="1rem" className="mb-2" />
                    <Skeleton width="100%" height="2.5rem" />
                </Card>
            </div>
        ));

        return skeletonItems;
    };

    const header = () => {
        return (
            <Toolbar
                left={() => (
                    <div className="flex align-items-center gap-2">
                        <h3 className="m-0">Bana Atanan Eğitimler</h3>
                        {!loading && (
                            <Badge value={filteredEgitimler.length} className="ml-2" />
                        )}
                    </div>
                )}
                right={() => (
                    <div className="flex align-items-center gap-2">
                        <span className="p-input-icon-left">
                            <i className="pi pi-search" />
                            <InputText 
                                placeholder="Eğitim ara..." 
                                value={globalFilter} 
                                onChange={(e) => setGlobalFilter(e.target.value)}
                                className="w-20rem"
                            />
                        </span>
                        
                        <Dropdown
                            value={durumFilter}
                            options={durumOptions}
                            onChange={(e) => setDurumFilter(e.value)}
                            placeholder="Durum"
                            className="w-10rem"
                            showClear
                        />

                        <DataViewLayoutOptions 
                            layout={layout} 
                            onChange={(e) => setLayout(e.value)} 
                        />
                    </div>
                )}
                className="mb-4"
            />
        );
    };

    return (
        <div className="card">
            <Toast ref={toast} />
            
            {header()}
            
            {loading ? (
                <div className="grid">
                    {renderSkeleton()}
                </div>
            ) : (
                <DataView
                    value={filteredEgitimler}
                    layout={layout}
                    itemTemplate={layout === 'grid' ? renderGridItem : renderListItem}
                    emptyMessage={renderEmpty()}
                    paginator={filteredEgitimler.length > 12}
                    rows={12}
                    rowsPerPageOptions={[12, 24, 48]}
                />
            )}
        </div>
    );
};

export default BanaAtananEgitimler;