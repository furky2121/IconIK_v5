import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { Chart } from 'primereact/chart';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Badge } from 'primereact/badge';
import { ProgressBar } from 'primereact/progressbar';
import { Panel } from 'primereact/panel';
import { TabView, TabPanel } from 'primereact/tabview';
import { Toast } from 'primereact/toast';
import { Skeleton } from 'primereact/skeleton';
import { Tag } from 'primereact/tag';
import { Timeline } from 'primereact/timeline';
import { Divider } from 'primereact/divider';
import iseAlimSureciService from '../services/iseAlimSureciService';

const IseAlimSurecleri = () => {
    const [dashboardData, setDashboardData] = useState(null);
    const [istatistikData, setIstatistikData] = useState(null);
    const [aktifSurecler, setAktifSurecler] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeIndex, setActiveIndex] = useState(0);
    const toast = useRef(null);

    useEffect(() => {
        loadAllData();
    }, []);

    const loadAllData = async () => {
        try {
            setLoading(true);

            const [dashboardResponse, istatistikResponse, sureclerResponse] = await Promise.all([
                iseAlimSureciService.getDashboard(),
                iseAlimSureciService.getIstatistikler(),
                iseAlimSureciService.getAktifSurecler()
            ]);

            if (dashboardResponse.success) {
                setDashboardData(dashboardResponse.data);
            }

            if (istatistikResponse.success) {
                setIstatistikData(istatistikResponse.data);
            }

            if (sureclerResponse.success) {
                setAktifSurecler(sureclerResponse.data);
            }
        } catch (error) {
            // console.error('Veri yükleme hatası:', error);
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Veriler yüklenirken bir hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const formatDate = (date) => {
        if (!date) return '-';
        return new Date(date).toLocaleDateString('tr-TR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const formatCurrency = (value) => {
        if (!value) return '-';
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(value);
    };

    const getDurumSeverity = (durum) => {
        switch (durum) {
            case 'YeniBasvuru': return 'info';
            case 'Degerlendiriliyor': return 'warning';
            case 'MulakatBekleniyor': return 'help';
            case 'MulakatTamamlandi': return 'secondary';
            case 'TeklifVerildi': return 'success';
            case 'IseAlindi': return 'success';
            case 'Reddedildi': return 'danger';
            case 'AdayVazgecti': return 'contrast';
            default: return 'info';
        }
    };

    const getDurumLabel = (durum) => {
        switch (durum) {
            case 'YeniBasvuru': return 'Yeni Başvuru';
            case 'Degerlendiriliyor': return 'Değerlendiriliyor';
            case 'MulakatBekleniyor': return 'Mülakat Bekliyor';
            case 'MulakatTamamlandi': return 'Mülakat Tamamlandı';
            case 'TeklifVerildi': return 'Teklif Verildi';
            case 'IseAlindi': return 'İşe Alındı';
            case 'Reddedildi': return 'Reddedildi';
            case 'AdayVazgecti': return 'Aday Vazgeçti';
            default: return durum;
        }
    };

    const durumBodyTemplate = (rowData) => {
        return <Tag value={getDurumLabel(rowData.durum)} severity={getDurumSeverity(rowData.durum)} />;
    };

    const adayBodyTemplate = (rowData) => {
        return (
            <div>
                <div className="font-medium">{rowData.adayBilgi?.adSoyad}</div>
                <div className="text-sm text-600">{rowData.adayBilgi?.email}</div>
            </div>
        );
    };

    const ilanBodyTemplate = (rowData) => {
        return (
            <div>
                <div className="font-medium">{rowData.ilanBilgi?.baslik}</div>
                <div className="text-sm text-600">{rowData.ilanBilgi?.pozisyon} - {rowData.ilanBilgi?.departman}</div>
            </div>
        );
    };

    const puanBodyTemplate = (rowData) => {
        if (!rowData.puan) return '-';
        return (
            <div className="flex align-items-center gap-2">
                <ProgressBar value={rowData.puan} style={{ width: '60px', height: '8px' }} />
                <span className="text-sm">{rowData.puan}/100</span>
            </div>
        );
    };

    const maasBodyTemplate = (rowData) => {
        return rowData.beklenenMaas ? formatCurrency(rowData.beklenenMaas) : '-';
    };

    const tarihBodyTemplate = (rowData) => {
        return formatDate(rowData.basvuruTarihi);
    };

    const sonrakiAdimBodyTemplate = (rowData) => {
        return <Badge value={rowData.sonrakiAdim} severity="info" />;
    };

    // Chart data
    const durumChartData = dashboardData?.durumBazindaBasvuru ? {
        labels: dashboardData.durumBazindaBasvuru.map(item => getDurumLabel(item.durum)),
        datasets: [{
            label: 'Başvuru Sayısı',
            data: dashboardData.durumBazindaBasvuru.map(item => item.sayi),
            backgroundColor: [
                '#3B82F6', '#F59E0B', '#8B5CF6', '#6B7280',
                '#10B981', '#10B981', '#EF4444', '#374151'
            ]
        }]
    } : null;

    const trendChartData = dashboardData?.aylikTrend ? {
        labels: dashboardData.aylikTrend.map(item => item.ay),
        datasets: [{
            label: 'Başvuru Sayısı',
            data: dashboardData.aylikTrend.map(item => item.sayi),
            borderColor: '#3B82F6',
            backgroundColor: 'rgba(59, 130, 246, 0.1)',
            tension: 0.4,
            fill: true
        }]
    } : null;

    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'bottom'
            }
        }
    };

    if (loading) {
        return (
            <div className="grid">
                <div className="col-12">
                    <div className="grid">
                        {[1, 2, 3, 4].map(i => (
                            <div key={i} className="col-12 md:col-6 lg:col-3">
                                <Card className="border-1 border-50">
                                    <Skeleton height="4rem" />
                                </Card>
                            </div>
                        ))}
                    </div>
                    <Skeleton height="20rem" className="mt-3" />
                </div>
            </div>
        );
    }

    return (
        <div className="grid">
            <Toast ref={toast} />

            <div className="col-12">
                <div className="flex align-items-center justify-content-between mb-4">
                    <h2 className="text-2xl font-bold text-900 m-0">İşe Alım Süreçleri</h2>
                    <Button
                        icon="pi pi-refresh"
                        label="Yenile"
                        onClick={loadAllData}
                        loading={loading}
                        size="small"
                    />
                </div>

                {/* Özet Kartları */}
                <div className="grid mb-4">
                    <div className="col-12 md:col-6 lg:col-3">
                        <Card className="border-1 border-blue-200 shadow-2">
                            <div className="flex align-items-center">
                                <div className="flex-1">
                                    <div className="text-sm text-600 mb-1">Aktif İlanlar</div>
                                    <div className="text-2xl font-bold text-blue-600">
                                        {dashboardData?.ozet?.aktifIlanlar || 0}
                                    </div>
                                </div>
                                <div className="border-circle bg-blue-100 p-3">
                                    <i className="pi pi-megaphone text-blue-600 text-xl" />
                                </div>
                            </div>
                        </Card>
                    </div>

                    <div className="col-12 md:col-6 lg:col-3">
                        <Card className="border-1 border-green-200 shadow-2">
                            <div className="flex align-items-center">
                                <div className="flex-1">
                                    <div className="text-sm text-600 mb-1">Toplam Başvuru</div>
                                    <div className="text-2xl font-bold text-green-600">
                                        {dashboardData?.ozet?.toplamBasvuru || 0}
                                    </div>
                                </div>
                                <div className="border-circle bg-green-100 p-3">
                                    <i className="pi pi-users text-green-600 text-xl" />
                                </div>
                            </div>
                        </Card>
                    </div>

                    <div className="col-12 md:col-6 lg:col-3">
                        <Card className="border-1 border-orange-200 shadow-2">
                            <div className="flex align-items-center">
                                <div className="flex-1">
                                    <div className="text-sm text-600 mb-1">Bu Ay Başvuru</div>
                                    <div className="text-2xl font-bold text-orange-600">
                                        {dashboardData?.ozet?.buAyBasvuru || 0}
                                    </div>
                                </div>
                                <div className="border-circle bg-orange-100 p-3">
                                    <i className="pi pi-calendar text-orange-600 text-xl" />
                                </div>
                            </div>
                        </Card>
                    </div>

                    <div className="col-12 md:col-6 lg:col-3">
                        <Card className="border-1 border-purple-200 shadow-2">
                            <div className="flex align-items-center">
                                <div className="flex-1">
                                    <div className="text-sm text-600 mb-1">Bekleyen Mülakat</div>
                                    <div className="text-2xl font-bold text-purple-600">
                                        {dashboardData?.ozet?.bekleyenMulakatlar || 0}
                                    </div>
                                </div>
                                <div className="border-circle bg-purple-100 p-3">
                                    <i className="pi pi-clock text-purple-600 text-xl" />
                                </div>
                            </div>
                        </Card>
                    </div>
                </div>

                {/* Tablar */}
                <TabView activeIndex={activeIndex} onTabChange={(e) => setActiveIndex(e.index)}>
                    <TabPanel header="Genel Bakış" leftIcon="pi pi-chart-line mr-2">
                        <div className="grid">
                            <div className="col-12 lg:col-6">
                                <Card title="Başvuru Durumları" className="h-full">
                                    {durumChartData && (
                                        <Chart
                                            type="doughnut"
                                            data={durumChartData}
                                            options={chartOptions}
                                            style={{ height: '300px' }}
                                        />
                                    )}
                                </Card>
                            </div>

                            <div className="col-12 lg:col-6">
                                <Card title="Aylık Başvuru Trendi" className="h-full">
                                    {trendChartData && (
                                        <Chart
                                            type="line"
                                            data={trendChartData}
                                            options={chartOptions}
                                            style={{ height: '300px' }}
                                        />
                                    )}
                                </Card>
                            </div>

                            <div className="col-12">
                                <Card title="En Çok Başvuru Alan İlanlar">
                                    <DataTable
                                        value={dashboardData?.enCokBasvuruAlanIlanlar || []}
                                        paginator
                                        rows={5}
                                        emptyMessage="Veri bulunamadı"
                                    >
                                        <Column field="baslik" header="İlan Başlığı" />
                                        <Column field="pozisyon" header="Pozisyon" />
                                        <Column field="departman" header="Departman" />
                                        <Column field="basvuruSayisi" header="Başvuru Sayısı" />
                                        <Column
                                            field="yayinTarihi"
                                            header="Yayın Tarihi"
                                            body={(rowData) => formatDate(rowData.yayinTarihi)}
                                        />
                                    </DataTable>
                                </Card>
                            </div>
                        </div>
                    </TabPanel>

                    <TabPanel header="Aktif Süreçler" leftIcon="pi pi-cog mr-2">
                        <Card title="Devam Eden İşe Alım Süreçleri">
                            <DataTable
                                value={aktifSurecler}
                                paginator
                                rows={10}
                                emptyMessage="Aktif süreç bulunamadı"
                                filterDisplay="menu"
                                globalFilterFields={['adayBilgi.adSoyad', 'ilanBilgi.baslik', 'durum']}
                            >
                                <Column
                                    field="adayBilgi"
                                    header="Aday"
                                    body={adayBodyTemplate}
                                    sortable
                                />
                                <Column
                                    field="ilanBilgi"
                                    header="İlan"
                                    body={ilanBodyTemplate}
                                />
                                <Column
                                    field="basvuruTarihi"
                                    header="Başvuru Tarihi"
                                    body={tarihBodyTemplate}
                                    sortable
                                />
                                <Column
                                    field="durum"
                                    header="Durum"
                                    body={durumBodyTemplate}
                                    sortable
                                />
                                <Column
                                    field="puan"
                                    header="Puan"
                                    body={puanBodyTemplate}
                                    sortable
                                />
                                <Column
                                    field="beklenenMaas"
                                    header="Beklenen Maaş"
                                    body={maasBodyTemplate}
                                    sortable
                                />
                                <Column
                                    field="sonrakiAdim"
                                    header="Sonraki Adım"
                                    body={sonrakiAdimBodyTemplate}
                                />
                            </DataTable>
                        </Card>
                    </TabPanel>

                    <TabPanel header="Son Aktiviteler" leftIcon="pi pi-history mr-2">
                        <div className="grid">
                            <div className="col-12 lg:col-6">
                                <Card title="Son Başvurular">
                                    <DataTable
                                        value={dashboardData?.sonBasvurular || []}
                                        paginator
                                        rows={10}
                                        emptyMessage="Veri bulunamadı"
                                    >
                                        <Column field="adayAd" header="Aday" />
                                        <Column field="pozisyon" header="Pozisyon" />
                                        <Column
                                            field="basvuruTarihi"
                                            header="Başvuru Tarihi"
                                            body={(rowData) => formatDate(rowData.basvuruTarihi)}
                                        />
                                        <Column
                                            field="durum"
                                            header="Durum"
                                            body={(rowData) => (
                                                <Tag
                                                    value={getDurumLabel(rowData.durum)}
                                                    severity={getDurumSeverity(rowData.durum)}
                                                />
                                            )}
                                        />
                                    </DataTable>
                                </Card>
                            </div>

                            <div className="col-12 lg:col-6">
                                <Card title="Bu Hafta Mülakatlar">
                                    <DataTable
                                        value={dashboardData?.buHaftaMulakatlar || []}
                                        paginator
                                        rows={10}
                                        emptyMessage="Mülakat bulunamadı"
                                    >
                                        <Column field="adayAd" header="Aday" />
                                        <Column field="tur" header="Tür" />
                                        <Column
                                            field="tarih"
                                            header="Tarih"
                                            body={(rowData) => formatDate(rowData.tarih)}
                                        />
                                        <Column field="mulakatYapan" header="Mülakat Yapan" />
                                        <Column field="durum" header="Durum" />
                                    </DataTable>
                                </Card>
                            </div>
                        </div>
                    </TabPanel>
                </TabView>
            </div>
        </div>
    );
};

export default IseAlimSurecleri;