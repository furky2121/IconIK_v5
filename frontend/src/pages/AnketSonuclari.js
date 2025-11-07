import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { Dropdown } from 'primereact/dropdown';
import { Toast } from 'primereact/toast';
import { Chart } from 'primereact/chart';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Tag } from 'primereact/tag';
import { Button } from 'primereact/button';
import { Toolbar } from 'primereact/toolbar';
import { ProgressBar } from 'primereact/progressbar';
import { SelectButton } from 'primereact/selectbutton';
import { Panel } from 'primereact/panel';
import { Skeleton } from 'primereact/skeleton';
import { Calendar } from 'primereact/calendar';
import { Divider } from 'primereact/divider';
import anketService from '../services/anketService';
import yetkiService from '../services/yetkiService';
import jsPDF from 'jspdf';
import 'jspdf-autotable';
import * as XLSX from 'xlsx';

const AnketSonuclari = () => {
    const [anketler, setAnketler] = useState([]);
    const [selectedAnketId, setSelectedAnketId] = useState(null);
    const [sonuclar, setSonuclar] = useState(null);
    const [istatistikler, setIstatistikler] = useState(null);
    const [katilimcilar, setKatilimcilar] = useState([]);
    const [loading, setLoading] = useState(false);
    const [permissions, setPermissions] = useState({ read: false });
    const [chartType, setChartType] = useState('pie');
    const [showFilters, setShowFilters] = useState(false);
    const [dateRange, setDateRange] = useState(null);
    const [selectedDurum, setSelectedDurum] = useState(null);
    const toast = useRef(null);

    const chartTypes = [
        { label: 'Pasta', value: 'pie', icon: 'pi pi-chart-pie' },
        { label: 'Çubuk', value: 'bar', icon: 'pi pi-chart-bar' },
        { label: 'Halka', value: 'doughnut', icon: 'pi pi-circle' }
    ];

    useEffect(() => {
        loadPermissions();
        loadAnketler();
    }, []);

    useEffect(() => {
        if (selectedAnketId) {
            loadAnketSonuclari();
            loadKatilimIstatistikleri();
            loadKatilimciDetaylari();
        }
    }, [selectedAnketId]);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('anketler', 'read')
            });
        } catch (error) {
            // Silent fail
        }
    };

    const loadAnketler = async () => {
        try {
            const response = await anketService.getAllAnketler();
            if (response.success) {
                setAnketler(response.data);
                if (response.data.length > 0) {
                    setSelectedAnketId(response.data[0].id);
                }
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Anketler yüklenirken hata oluştu',
                life: 3000
            });
        }
    };

    const loadAnketSonuclari = async () => {
        if (!selectedAnketId) return;

        setLoading(true);
        try {
            const response = await anketService.getAnketSonuclari(selectedAnketId);

            if (response.success) {
                setSonuclar(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Sonuçlar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const loadKatilimIstatistikleri = async () => {
        if (!selectedAnketId) return;

        try {
            const response = await anketService.getKatilimIstatistikleri(selectedAnketId);
            if (response.success) {
                setIstatistikler(response.data);
            }
        } catch (error) {
            // Silent fail
        }
    };

    const loadKatilimciDetaylari = async () => {
        if (!selectedAnketId) return;

        try {
            const response = await anketService.getAnketCevaplari(selectedAnketId);
            if (response.success) {
                setKatilimcilar(response.data || []);
            }
        } catch (error) {
            // Silent fail
        }
    };

    // Export Functions
    const exportToPDF = () => {
        const doc = new jsPDF();

        // Header
        doc.setFontSize(18);
        doc.text('Anket Sonuçları Raporu', 14, 22);

        doc.setFontSize(11);
        doc.text(sonuclar?.anketBaslik || '', 14, 32);
        doc.text(`Rapor Tarihi: ${new Date().toLocaleDateString('tr-TR')}`, 14, 38);

        let yPos = 48;

        // İstatistikler
        if (istatistikler) {
            doc.setFontSize(14);
            doc.text('Katılım İstatistikleri', 14, yPos);
            yPos += 10;

            const statsData = [
                ['Hedef Kişi', istatistikler.hedefKisiSayisi],
                ['Toplam Katılım', istatistikler.katilimSayisi],
                ['Tamamlanan', istatistikler.tamamlananSayisi],
                ['Katılım Oranı', `%${istatistikler.katilimOrani}`]
            ];

            doc.autoTable({
                startY: yPos,
                head: [['Metrik', 'Değer']],
                body: statsData,
                theme: 'grid',
                headStyles: { fillColor: [66, 165, 245] }
            });

            yPos = doc.lastAutoTable.finalY + 15;
        }

        // Sorular ve Cevaplar
        if (sonuclar?.sorular) {
            sonuclar.sorular.forEach((soru, index) => {
                if (yPos > 250) {
                    doc.addPage();
                    yPos = 20;
                }

                doc.setFontSize(12);
                doc.text(`Soru ${index + 1}: ${soru.soruMetni}`, 14, yPos);
                yPos += 8;

                if (soru.soruTipi === 'AcikUclu') {
                    doc.setFontSize(10);
                    doc.text(`Toplam ${soru.toplamCevap} açık uçlu cevap`, 14, yPos);
                    yPos += 10;
                } else {
                    const tableData = soru.secenekler.map(s => [
                        s.secenekMetni,
                        s.cevapSayisi,
                        `%${s.yuzde}`
                    ]);

                    doc.autoTable({
                        startY: yPos,
                        head: [['Seçenek', 'Sayı', 'Oran']],
                        body: tableData,
                        theme: 'striped'
                    });

                    yPos = doc.lastAutoTable.finalY + 15;
                }
            });
        }

        doc.save(`anket-sonuclari-${selectedAnketId}.pdf`);

        toast.current.show({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'PDF başarıyla indirildi',
            life: 3000
        });
    };

    const exportToExcel = () => {
        const wb = XLSX.utils.book_new();

        // İstatistikler sayfası
        if (istatistikler) {
            const statsData = [
                ['Metrik', 'Değer'],
                ['Anket Başlığı', sonuclar?.anketBaslik || ''],
                ['Hedef Kişi Sayısı', istatistikler.hedefKisiSayisi],
                ['Toplam Katılım', istatistikler.katilimSayisi],
                ['Tamamlanan', istatistikler.tamamlananSayisi],
                ['Katılım Oranı', `%${istatistikler.katilimOrani}`]
            ];
            const ws1 = XLSX.utils.aoa_to_sheet(statsData);
            XLSX.utils.book_append_sheet(wb, ws1, 'İstatistikler');
        }

        // Her soru için ayrı sayfa
        if (sonuclar?.sorular) {
            sonuclar.sorular.forEach((soru, index) => {
                if (soru.soruTipi !== 'AcikUclu') {
                    const questionData = [
                        [`Soru ${index + 1}: ${soru.soruMetni}`],
                        [],
                        ['Seçenek', 'Cevap Sayısı', 'Yüzde'],
                        ...soru.secenekler.map(s => [s.secenekMetni, s.cevapSayisi, `%${s.yuzde}`])
                    ];
                    const ws = XLSX.utils.aoa_to_sheet(questionData);
                    XLSX.utils.book_append_sheet(wb, ws, `Soru ${index + 1}`);
                }
            });
        }

        // Katılımcılar sayfası
        if (katilimcilar && katilimcilar.length > 0) {
            const participantData = [
                ['Personel', 'Durum', 'Başlangıç', 'Tamamlanma'],
                ...katilimcilar.map(k => [
                    k.personelAd || 'Anonim',
                    k.durum,
                    k.baslangicTarihi ? anketService.formatDateTime(k.baslangicTarihi) : '-',
                    k.tamamlanmaTarihi ? anketService.formatDateTime(k.tamamlanmaTarihi) : '-'
                ])
            ];
            const ws = XLSX.utils.aoa_to_sheet(participantData);
            XLSX.utils.book_append_sheet(wb, ws, 'Katılımcılar');
        }

        XLSX.writeFile(wb, `anket-sonuclari-${selectedAnketId}.xlsx`);

        toast.current.show({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Excel başarıyla indirildi',
            life: 3000
        });
    };

    const getChartData = (soru, type) => {
        const labels = soru.secenekler.map(s => s.secenekMetni);
        const data = soru.secenekler.map(s => s.cevapSayisi);
        const backgroundColors = [
            '#42A5F5', '#66BB6A', '#FFA726', '#AB47BC',
            '#26C6DA', '#FFCA28', '#EF5350', '#78909C'
        ];

        if (type === 'bar') {
            return {
                labels,
                datasets: [{
                    label: 'Cevap Sayısı',
                    data,
                    backgroundColor: backgroundColors.slice(0, labels.length),
                }]
            };
        }

        return {
            labels,
            datasets: [{
                data,
                backgroundColor: backgroundColors.slice(0, labels.length),
                hoverBackgroundColor: backgroundColors.slice(0, labels.length)
            }]
        };
    };

    const getChartOptions = (type) => {
        const baseOptions = {
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 15
                    }
                }
            }
        };

        if (type === 'bar') {
            return {
                ...baseOptions,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                }
            };
        }

        return baseOptions;
    };

    const renderSoruSonucu = (soru, index) => {
        if (soru.soruTipi === 'AcikUclu') {
            return (
                <Card key={soru.soruId} className="mb-4">
                    <div className="flex justify-content-between align-items-center mb-3">
                        <h4 className="m-0">Soru {index + 1}: {soru.soruMetni}</h4>
                        <Tag value={`${soru.toplamCevap} Cevap`} severity="info" />
                    </div>

                    <DataTable
                        value={soru.acikCevaplar}
                        paginator
                        rows={5}
                        responsiveLayout="scroll"
                        emptyMessage="Henüz cevap yok"
                        className="mt-3"
                    >
                        {!sonuclar?.anonymousMu && (
                            <Column
                                field="personelAd"
                                header="Personel"
                                sortable
                                style={{ width: '200px' }}
                            ></Column>
                        )}
                        <Column
                            field="cevap"
                            header="Cevap"
                            style={{ minWidth: '300px' }}
                        ></Column>
                        <Column
                            field="tarih"
                            header="Tarih"
                            body={(rowData) => anketService.formatDateTime(rowData.tarih)}
                            sortable
                            style={{ width: '180px' }}
                        ></Column>
                    </DataTable>
                </Card>
            );
        }

        // Çoktan seçmeli soru
        // Seçenekleri kontrol et
        if (!soru.secenekler || soru.secenekler.length === 0) {
            return (
                <Card key={soru.soruId} className="mb-4">
                    <div className="flex justify-content-between align-items-start mb-3">
                        <div>
                            <h4 className="m-0 mb-2">Soru {index + 1}: {soru.soruMetni}</h4>
                            <div className="flex gap-2">
                                <Tag value={anketService.getSoruTipiLabel(soru.soruTipi)} severity="info" />
                                <Tag value={`${soru.toplamCevap || 0} Cevap`} severity="success" />
                            </div>
                        </div>
                    </div>
                    <div className="text-center p-4 text-500">
                        <i className="pi pi-info-circle mr-2"></i>
                        Henüz cevap yok veya seçenek tanımlanmamış
                    </div>
                </Card>
            );
        }

        // Tüm cevaplar 0 mı kontrol et
        const toplamCevapSayisi = soru.secenekler.reduce((sum, s) => sum + (s.cevapSayisi || 0), 0);
        if (toplamCevapSayisi === 0) {
            return (
                <Card key={soru.soruId} className="mb-4">
                    <div className="flex justify-content-between align-items-start mb-3">
                        <div>
                            <h4 className="m-0 mb-2">Soru {index + 1}: {soru.soruMetni}</h4>
                            <div className="flex gap-2">
                                <Tag value={anketService.getSoruTipiLabel(soru.soruTipi)} severity="info" />
                                <Tag value="Henüz Cevap Yok" severity="warning" />
                            </div>
                        </div>
                    </div>
                    <DataTable
                        value={soru.secenekler}
                        responsiveLayout="scroll"
                        emptyMessage="Seçenek bulunamadı"
                    >
                        <Column field="secenekMetni" header="Seçenek"></Column>
                        <Column
                            field="cevapSayisi"
                            header="Cevap Sayısı"
                            body={() => <span className="text-500">0</span>}
                            style={{ width: '150px', textAlign: 'center' }}
                        ></Column>
                    </DataTable>
                    <div className="text-center p-3 mt-3 text-500 bg-blue-50 border-round">
                        <i className="pi pi-info-circle mr-2"></i>
                        Bu soru için henüz hiçbir seçenek işaretlenmemiş
                    </div>
                </Card>
            );
        }

        const chartData = getChartData(soru, chartType);
        const chartOptions = getChartOptions(chartType);

        return (
            <Card key={soru.soruId} className="mb-4">
                <div className="flex justify-content-between align-items-start mb-3">
                    <div>
                        <h4 className="m-0 mb-2">Soru {index + 1}: {soru.soruMetni}</h4>
                        <div className="flex gap-2">
                            <Tag value={anketService.getSoruTipiLabel(soru.soruTipi)} severity="info" />
                            {soru.soruTipi === 'CokluSecim' && soru.toplamKatilimci ? (
                                <>
                                    <Tag value={`${soru.toplamKatilimci} Katılımcı`} severity="success" />
                                    <Tag value={`${soru.toplamCevap} Toplam Seçim`} severity="warning" />
                                </>
                            ) : (
                                <Tag value={`${soru.toplamCevap || 0} Cevap`} severity="success" />
                            )}
                        </div>
                    </div>
                </div>

                <div className="grid mt-4">
                    <div className="col-12 lg:col-6">
                        <div style={{ height: '300px', position: 'relative' }}>
                            <Chart type={chartType} data={chartData} options={chartOptions} />
                        </div>
                    </div>
                    <div className="col-12 lg:col-6">
                        <DataTable
                            value={soru.secenekler}
                            responsiveLayout="scroll"
                            emptyMessage="Seçenek bulunamadı"
                        >
                            <Column field="secenekMetni" header="Seçenek"></Column>
                            <Column
                                field="cevapSayisi"
                                header="Sayı"
                                sortable
                                style={{ width: '100px', textAlign: 'center' }}
                            ></Column>
                            <Column
                                field="yuzde"
                                header="Oran"
                                body={(rowData) => (
                                    <div>
                                        <div className="mb-2">%{rowData.yuzde || 0}</div>
                                        <ProgressBar
                                            value={rowData.yuzde || 0}
                                            showValue={false}
                                            style={{ height: '8px' }}
                                        />
                                    </div>
                                )}
                                style={{ width: '150px' }}
                            ></Column>
                        </DataTable>
                    </div>
                </div>
            </Card>
        );
    };

    const renderStatCard = (title, value, icon, color, total, subtitle) => {
        const percentage = total ? Math.round((value / total) * 100) : 0;

        return (
            <Card className="shadow-2" style={{ height: '100%', minHeight: '140px' }}>
                <div className="flex align-items-start">
                    <div
                        className="flex align-items-center justify-content-center border-circle"
                        style={{
                            width: '3.5rem',
                            height: '3.5rem',
                            backgroundColor: `${color}20`,
                            color: color
                        }}
                    >
                        <i className={`${icon} text-2xl`}></i>
                    </div>
                    <div className="ml-3 flex-1">
                        <div className="text-500 text-sm mb-1">{title}</div>
                        <div className={`text-3xl font-bold mb-1`} style={{ color: color }}>
                            {value}
                        </div>
                        {subtitle && (
                            <div className="text-600 text-sm">{subtitle}</div>
                        )}
                        {total && (
                            <ProgressBar
                                value={percentage}
                                showValue={false}
                                className="mt-2"
                                style={{ height: '6px' }}
                                color={color}
                            />
                        )}
                    </div>
                </div>
            </Card>
        );
    };

    const renderLoadingSkeleton = () => (
        <div>
            <Skeleton height="4rem" className="mb-3" />
            <div className="grid">
                {[1, 2, 3, 4].map(i => (
                    <div key={i} className="col-12 md:col-3">
                        <Skeleton height="8rem" />
                    </div>
                ))}
            </div>
            <Skeleton height="20rem" className="mt-4" />
        </div>
    );

    const toolbarLeftContent = (
        <div className="flex align-items-center gap-2">
            <h3 className="m-0">Anket Sonuçları</h3>
            {sonuclar?.anonymousMu && (
                <Tag value="Anonim Anket" severity="info" icon="pi pi-lock" />
            )}
        </div>
    );

    const toolbarRightContent = (
        <div className="flex gap-2">
            <SelectButton
                value={chartType}
                options={chartTypes}
                onChange={(e) => setChartType(e.value)}
                itemTemplate={(option) => <i className={option.icon}></i>}
            />
            <Button
                label="PDF"
                icon="pi pi-file-pdf"
                className="p-button-danger"
                onClick={exportToPDF}
                disabled={!sonuclar}
            />
            <Button
                label="Excel"
                icon="pi pi-file-excel"
                className="p-button-success"
                onClick={exportToExcel}
                disabled={!sonuclar}
            />
            <Button
                icon="pi pi-print"
                className="p-button-secondary"
                onClick={() => window.print()}
                disabled={!sonuclar}
                tooltip="Yazdır"
            />
        </div>
    );

    if (!permissions.read) {
        return (
            <div className="flex align-items-center justify-content-center" style={{ minHeight: '400px' }}>
                <Card>
                    <div className="text-center">
                        <i className="pi pi-lock" style={{ fontSize: '3rem', color: 'var(--primary-color)' }}></i>
                        <h3>Yetkiniz Yok</h3>
                        <p>Bu sayfayı görüntülemek için yetkiniz bulunmamaktadır.</p>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <Toast ref={toast} />

                <Toolbar left={toolbarLeftContent} right={toolbarRightContent} className="mb-4" />

                <Card className="mb-4">
                    <div className="formgrid grid">
                        <div className="field col-12 md:col-6">
                            <label htmlFor="anket-select" className="block mb-2 font-semibold">
                                <i className="pi pi-list mr-2"></i>
                                Anket Seçin
                            </label>
                            <Dropdown
                                id="anket-select"
                                value={selectedAnketId}
                                options={anketler}
                                onChange={(e) => setSelectedAnketId(e.value)}
                                optionLabel="baslik"
                                optionValue="id"
                                placeholder="Bir anket seçin"
                                className="w-full"
                                filter
                            />
                        </div>
                    </div>
                </Card>

                {loading ? (
                    renderLoadingSkeleton()
                ) : istatistikler ? (
                    <>
                        <div className="grid mb-4">
                            <div className="col-12 md:col-3">
                                {renderStatCard(
                                    'Hedef Kişi',
                                    istatistikler.hedefKisiSayisi,
                                    'pi pi-users',
                                    '#6366F1'
                                )}
                            </div>
                            <div className="col-12 md:col-3">
                                {renderStatCard(
                                    'Toplam Katılım',
                                    istatistikler.katilimSayisi,
                                    'pi pi-user-plus',
                                    '#3B82F6',
                                    istatistikler.hedefKisiSayisi,
                                    `%${istatistikler.katilimOrani} katılım`
                                )}
                            </div>
                            <div className="col-12 md:col-3">
                                {renderStatCard(
                                    'Tamamlanan',
                                    istatistikler.tamamlananSayisi,
                                    'pi pi-check-circle',
                                    '#10B981',
                                    istatistikler.katilimSayisi,
                                    `${istatistikler.katilimSayisi - istatistikler.tamamlananSayisi} devam ediyor`
                                )}
                            </div>
                            <div className="col-12 md:col-3">
                                {renderStatCard(
                                    'Başarı Oranı',
                                    `%${istatistikler.katilimOrani}`,
                                    'pi pi-chart-line',
                                    '#F59E0B'
                                )}
                            </div>
                        </div>

                        {/* Katılımcı Detayları */}
                        {katilimcilar && katilimcilar.length > 0 && (
                            <Panel header="Katılımcı Detayları" toggleable collapsed className="mb-4">
                                <DataTable
                                    value={katilimcilar}
                                    paginator
                                    rows={10}
                                    responsiveLayout="scroll"
                                    emptyMessage="Henüz katılımcı yok"
                                >
                                    {!sonuclar?.anonymousMu && (
                                        <Column field="personelAd" header="Personel" sortable></Column>
                                    )}
                                    <Column
                                        field="durum"
                                        header="Durum"
                                        body={(rowData) => {
                                            const badge = anketService.getKatilimDurumuBadge(rowData.durum);
                                            return <Tag value={badge.label} severity={badge.severity} />;
                                        }}
                                        sortable
                                    ></Column>
                                    <Column
                                        field="baslangicTarihi"
                                        header="Başlangıç"
                                        body={(rowData) => anketService.formatDateTime(rowData.baslangicTarihi)}
                                        sortable
                                    ></Column>
                                    <Column
                                        field="tamamlanmaTarihi"
                                        header="Tamamlanma"
                                        body={(rowData) => rowData.tamamlanmaTarihi
                                            ? anketService.formatDateTime(rowData.tamamlanmaTarihi)
                                            : '-'
                                        }
                                        sortable
                                    ></Column>
                                </DataTable>
                            </Panel>
                        )}

                        <Divider />

                        {/* Soru Sonuçları */}
                        {sonuclar?.sorular && sonuclar.sorular.length > 0 ? (
                            <div>
                                <h3 className="mb-4">
                                    <i className="pi pi-chart-bar mr-2"></i>
                                    Soru Bazlı Analiz
                                </h3>
                                {sonuclar.sorular.map((soru, index) => renderSoruSonucu(soru, index))}
                            </div>
                        ) : (
                            <Card>
                                <div className="text-center p-5">
                                    <i className="pi pi-inbox" style={{ fontSize: '3rem', color: 'var(--text-color-secondary)' }}></i>
                                    <h3>Henüz Cevap Yok</h3>
                                    <p>Bu anket için henüz cevap bulunmamaktadır.</p>
                                </div>
                            </Card>
                        )}
                    </>
                ) : (
                    <Card>
                        <div className="text-center p-5">
                            <i className="pi pi-chart-bar" style={{ fontSize: '3rem', color: 'var(--text-color-secondary)' }}></i>
                            <h3>Bir anket seçin</h3>
                            <p>Sonuçları görüntülemek için yukarıdan bir anket seçin.</p>
                        </div>
                    </Card>
                )}
            </div>
        </div>
    );
};

export default AnketSonuclari;
