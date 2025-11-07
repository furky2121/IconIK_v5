import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Card } from 'primereact/card';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { ProgressBar } from 'primereact/progressbar';
import { Badge } from 'primereact/badge';
import { Chart } from 'primereact/chart';
import { TabView, TabPanel } from 'primereact/tabview';
import { Timeline } from 'primereact/timeline';
import { Toast } from 'primereact/toast';
import { Skeleton } from 'primereact/skeleton';
import { InputText } from 'primereact/inputtext';
import videoEgitimService from '../services/videoEgitimService';
import './BenimEgitimlerim.css';

const BenimEgitimlerim = () => {
    const router = useRouter();
    const toast = useRef(null);
    const [activeIndex, setActiveIndex] = useState(0);
    const [loading, setLoading] = useState(true);
    const [personalId, setPersonalId] = useState(null);
    
    // Data states
    const [benimEgitimlerim, setBenimEgitimlerim] = useState([]);
    const [bekleyenEgitimler, setBekleyenEgitimler] = useState([]);
    const [istatistikler, setIstatistikler] = useState({});
    const [chartData, setChartData] = useState({});
    const [timelineData, setTimelineData] = useState([]);
    
    // Filter states
    const [globalFilter, setGlobalFilter] = useState('');
    
    useEffect(() => {
        loadPersonalInfo();
    }, []);
    
    useEffect(() => {
        if (personalId) {
            loadAllData();
        }
    }, [personalId]);

    const loadPersonalInfo = () => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                setPersonalId(payload.personelId || payload.sub);
            } catch (error) {
            // console.error('Error parsing token:', error);
                toast.current?.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'Kullanıcı bilgileri alınamadı.'
                });
            }
        }
    };

    const loadAllData = async () => {
        setLoading(true);
        try {
            await Promise.all([
                loadBenimEgitimlerim(),
                loadBekleyenEgitimler(),
                loadIstatistikler(),
            ]);
        } catch (error) {
            // console.error('Error loading data:', error);
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Veriler yüklenirken hata oluştu.'
            });
        } finally {
            setLoading(false);
        }
    };

    const loadBenimEgitimlerim = async () => {
        try {
            const response = await videoEgitimService.getBenimEgitimlerim();
            if (response.success) {
                setBenimEgitimlerim(response.data);
                createTimelineData(response.data);
            }
        } catch (error) {
            // console.error('Error loading my trainings:', error);
        }
    };

    const loadBekleyenEgitimler = async () => {
        try {
            const response = await videoEgitimService.getBekleyenEgitimler();
            if (response.success) {
                setBekleyenEgitimler(response.data);
            }
        } catch (error) {
            // console.error('Error loading pending trainings:', error);
        }
    };

    const loadIstatistikler = async () => {
        try {
            const response = await videoEgitimService.getDetailedAnalytics({ personelId });
            if (response.success) {
                setIstatistikler(response.data);
                createChartData(response.data);
            }
        } catch (error) {
            // console.error('Error loading statistics:', error);
        }
    };

    const createChartData = (stats) => {
        // Progress chart data
        const progressData = {
            labels: ['Tamamlanan', 'Kalan'],
            datasets: [{
                data: [stats.tamamlananEgitim || 0, (stats.toplamEgitim || 0) - (stats.tamamlananEgitim || 0)],
                backgroundColor: ['#10B981', '#E5E7EB'],
                hoverBackgroundColor: ['#059669', '#D1D5DB']
            }]
        };

        // Monthly progress chart
        const monthlyData = {
            labels: ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'],
            datasets: [{
                label: 'Tamamlanan Eğitimler',
                data: new Array(12).fill(0), // This would be populated from real data
                borderColor: '#3B82F6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                tension: 0.4
            }]
        };

        setChartData({ progressData, monthlyData });
    };

    const createTimelineData = (egitimler) => {
        const tamamlananlar = egitimler
            .filter(e => e.tamamlandiMi)
            .sort((a, b) => new Date(b.tamamlanmaTarihi) - new Date(a.tamamlanmaTarihi))
            .slice(0, 10)
            .map(egitim => ({
                status: 'Tamamlandı',
                date: new Date(egitim.tamamlanmaTarihi).toLocaleDateString('tr-TR'),
                icon: 'pi pi-check',
                color: '#10B981',
                title: egitim.VideoEgitim?.baslik || egitim.baslik,
                description: `Süre: ${videoEgitimService.formatDuration(egitim.VideoEgitim?.sureDakika)}`
            }));

        setTimelineData(tamamlananlar);
    };

    // Template functions
    const thumbnailBodyTemplate = (rowData) => {
        const egitim = rowData.VideoEgitim || rowData;
        return (
            <img 
                src={egitim.thumbnailUrl || '/layout/images/icon_ik.png'} 
                alt={egitim.baslik}
                width="60" 
                height="40" 
                className="border-round cursor-pointer"
                onClick={() => handleWatchVideo(egitim.id)}
            />
        );
    };

    const titleBodyTemplate = (rowData) => {
        const egitim = rowData.VideoEgitim || rowData;
        return (
            <div>
                <h6 className="m-0 mb-1">{egitim.baslik}</h6>
                <small className="text-500">{egitim.egitmen}</small>
            </div>
        );
    };

    const progressBodyTemplate = (rowData) => {
        const progress = rowData.izlemeYuzdesi || 0;
        const required = rowData.VideoEgitim?.izlenmeMinimum || 80;
        const color = videoEgitimService.getProgressColor(progress);
        
        return (
            <div>
                <ProgressBar 
                    value={progress} 
                    style={{ height: '8px' }}
                    color={color}
                />
                <small className="text-500 mt-1">
                    {Math.round(progress)}% / {required}% gerekli
                </small>
            </div>
        );
    };

    const statusBodyTemplate = (rowData) => {
        if (rowData.tamamlandiMi) {
            return <Badge value="Tamamlandı" severity="success" />;
        } else if (rowData.izlemeYuzdesi > 0) {
            return <Badge value="Devam Ediyor" severity="warning" />;
        } else {
            return <Badge value="Başlanmadı" severity="info" />;
        }
    };

    const dueDateBodyTemplate = (rowData) => {
        const egitim = rowData.VideoEgitim || rowData;
        if (!egitim.sonTamamlanmaTarihi) return '-';
        
        const dueDate = new Date(egitim.sonTamamlanmaTarihi);
        const today = new Date();
        const daysLeft = Math.ceil((dueDate - today) / (1000 * 60 * 60 * 24));
        
        if (daysLeft < 0) {
            return <Badge value="Süresi Geçti" severity="danger" />;
        } else if (daysLeft <= 7) {
            return <Badge value={`${daysLeft} gün kaldı`} severity="warning" />;
        } else {
            return <span className="text-500">{dueDate.toLocaleDateString('tr-TR')}</span>;
        }
    };

    const actionBodyTemplate = (rowData) => {
        const egitim = rowData.VideoEgitim || rowData;
        return (
            <div className="flex gap-2">
                <Button 
                    icon="pi pi-play" 
                    className="p-button-rounded p-button-text"
                    onClick={() => handleWatchVideo(egitim.id)}
                    tooltip="Videoyu İzle"
                />
                {rowData.tamamlandiMi && (
                    <Button 
                        icon="pi pi-verified" 
                        className="p-button-rounded p-button-text p-button-success"
                        onClick={() => handleCreateCertificate(egitim.id)}
                        tooltip="Sertifika"
                    />
                )}
            </div>
        );
    };

    // Event handlers
    const handleWatchVideo = (egitimId) => {
        if (egitimId) {
            router.push(`/egitimler/izle/${egitimId}`);
        } else {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Video ID bulunamadı.'
            });
        }
    };

    const handleCreateCertificate = async (egitimId) => {
        try {
            const response = await videoEgitimService.sertifikaOlustur(egitimId);
            if (response.success) {
                toast.current?.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Sertifikanız oluşturuldu!'
                });
                // Reload data to update certificate status
                loadBenimEgitimlerim();
            }
        } catch (error) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Sertifika oluşturulamadı.'
            });
        }
    };

    // Header templates
    const header = (title) => (
        <div className="flex flex-column md:flex-row md:align-items-center md:justify-content-between gap-2">
            <h3 className="m-0">{title}</h3>
            <div className="flex gap-2">
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText 
                        type="search" 
                        onInput={(e) => setGlobalFilter(e.target.value)} 
                        placeholder="Ara..." 
                    />
                </span>
            </div>
        </div>
    );

    if (loading) {
        return (
            <div className="benim-egitimlerim-container">
                <div className="loading-skeleton">
                    <div className="grid">
                        <div className="col-12 lg:col-3">
                            <Skeleton height="150px" className="mb-3" />
                        </div>
                        <div className="col-12 lg:col-3">
                            <Skeleton height="150px" className="mb-3" />
                        </div>
                        <div className="col-12 lg:col-3">
                            <Skeleton height="150px" className="mb-3" />
                        </div>
                        <div className="col-12 lg:col-3">
                            <Skeleton height="150px" className="mb-3" />
                        </div>
                    </div>
                    <Skeleton height="400px" />
                </div>
            </div>
        );
    }

    return (
        <div className="benim-egitimlerim-container">
            <Toast ref={toast} />
            
            {/* Statistics Cards */}
            <div className="stats-grid mb-4">
                <Card className="stats-card stats-total">
                    <div className="stats-content">
                        <div className="stats-icon">
                            <i className="pi pi-play-circle"></i>
                        </div>
                        <div className="stats-info">
                            <h2 className="stats-number">{istatistikler.toplamEgitim || 0}</h2>
                            <p className="stats-label">Toplam Eğitim</p>
                        </div>
                    </div>
                </Card>
                
                <Card className="stats-card stats-completed">
                    <div className="stats-content">
                        <div className="stats-icon">
                            <i className="pi pi-check-circle"></i>
                        </div>
                        <div className="stats-info">
                            <h2 className="stats-number">{istatistikler.tamamlananEgitim || 0}</h2>
                            <p className="stats-label">Tamamlanan</p>
                        </div>
                    </div>
                </Card>
                
                <Card className="stats-card stats-progress">
                    <div className="stats-content">
                        <div className="stats-icon">
                            <i className="pi pi-percentage"></i>
                        </div>
                        <div className="stats-info">
                            <h2 className="stats-number">{istatistikler.completionRate || 0}%</h2>
                            <p className="stats-label">Tamamlama Oranı</p>
                        </div>
                    </div>
                </Card>
                
                <Card className="stats-card stats-time">
                    <div className="stats-content">
                        <div className="stats-icon">
                            <i className="pi pi-clock"></i>
                        </div>
                        <div className="stats-info">
                            <h2 className="stats-number">{istatistikler.averageWatchTimeHours || 0}</h2>
                            <p className="stats-label">Toplam Saat</p>
                        </div>
                    </div>
                </Card>
            </div>

            {/* Performance Badge */}
            {istatistikler.performanceLevel && (
                <Card className="performance-card mb-4">
                    <div className="performance-content">
                        <div className="performance-badge" style={{ backgroundColor: istatistikler.performanceColor }}>
                            <i className="pi pi-star"></i>
                            <span>Performans Seviyesi: {istatistikler.performanceLevel}</span>
                        </div>
                    </div>
                </Card>
            )}

            {/* Main Content */}
            <Card>
                <TabView activeIndex={activeIndex} onTabChange={(e) => setActiveIndex(e.index)}>
                    {/* All Trainings Tab */}
                    <TabPanel header="Tüm Eğitimlerim" leftIcon="pi pi-list">
                        <DataTable
                            value={benimEgitimlerim}
                            paginator
                            rows={10}
                            dataKey="id"
                            globalFilter={globalFilter}
                            header={header('Tüm Eğitimlerim')}
                            emptyMessage="Eğitim bulunamadı."
                            className="datatable-responsive"
                        >
                            <Column body={thumbnailBodyTemplate} header="Video" style={{ width: '80px' }} />
                            <Column body={titleBodyTemplate} header="Eğitim" sortable />
                            <Column body={progressBodyTemplate} header="İlerleme" />
                            <Column body={statusBodyTemplate} header="Durum" />
                            <Column body={dueDateBodyTemplate} header="Son Tarih" sortable />
                            <Column body={actionBodyTemplate} exportable={false} style={{ width: '120px' }} />
                        </DataTable>
                    </TabPanel>

                    {/* Pending Trainings Tab */}
                    <TabPanel header="Bekleyen Eğitimler" leftIcon="pi pi-clock" rightIcon={bekleyenEgitimler.length > 0 ? bekleyenEgitimler.length.toString() : null}>
                        <DataTable
                            value={bekleyenEgitimler}
                            paginator
                            rows={10}
                            dataKey="id"
                            globalFilter={globalFilter}
                            header={header('Bekleyen Eğitimler')}
                            emptyMessage="Bekleyen eğitim bulunamadı."
                            className="datatable-responsive"
                        >
                            <Column body={thumbnailBodyTemplate} header="Video" style={{ width: '80px' }} />
                            <Column body={titleBodyTemplate} header="Eğitim" sortable />
                            <Column body={dueDateBodyTemplate} header="Son Tarih" sortable />
                            <Column 
                                body={(rowData) => rowData.VideoEgitim?.zorunluMu ? <Badge value="Zorunlu" severity="danger" /> : <Badge value="Seçmeli" severity="info" />} 
                                header="Tip" 
                            />
                            <Column body={actionBodyTemplate} exportable={false} style={{ width: '120px' }} />
                        </DataTable>
                    </TabPanel>

                    {/* Progress Analytics Tab */}
                    <TabPanel header="İlerleme Analizi" leftIcon="pi pi-chart-line">
                        <div className="grid">
                            <div className="col-12 lg:col-6">
                                <Card title="Tamamlanma Durumu">
                                    {chartData.progressData && (
                                        <Chart type="doughnut" data={chartData.progressData} style={{ width: '100%', height: '300px' }} />
                                    )}
                                </Card>
                            </div>
                            <div className="col-12 lg:col-6">
                                <Card title="Aylık İlerleme">
                                    {chartData.monthlyData && (
                                        <Chart type="line" data={chartData.monthlyData} style={{ width: '100%', height: '300px' }} />
                                    )}
                                </Card>
                            </div>
                            <div className="col-12">
                                <Card title="Son Tamamlanan Eğitimler">
                                    <Timeline value={timelineData} content={(item) => (
                                        <div className="timeline-item">
                                            <h6 className="timeline-title">{item.title}</h6>
                                            <p className="timeline-date">{item.date}</p>
                                            <small className="timeline-description">{item.description}</small>
                                        </div>
                                    )} />
                                </Card>
                            </div>
                        </div>
                    </TabPanel>
                </TabView>
            </Card>
        </div>
    );
};

export default BenimEgitimlerim;