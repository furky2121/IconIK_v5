import React, { useState, useEffect, useRef } from 'react';
import { Button } from 'primereact/button';
import { InputNumber } from 'primereact/inputnumber';
import { Toast } from 'primereact/toast';
import { Card } from 'primereact/card';
import { Dropdown } from 'primereact/dropdown';
import { Panel } from 'primereact/panel';
import { Divider } from 'primereact/divider';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import bordroService from '../services/bordroService';
import departmanService from '../services/departmanService';
import yetkiService from '../services/yetkiService';

const BordroRaporlari = () => {
    const [filters, setFilters] = useState({
        yil: new Date().getFullYear(),
        ay: new Date().getMonth() + 1,
        departmanId: null
    });
    const [ozet, setOzet] = useState(null);
    const [departmanDagilim, setDepartmanDagilim] = useState([]);
    const [departmanlar, setDepartmanlar] = useState([]);
    const [loading, setLoading] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false
    });
    const toast = useRef(null);

    const ayOptions = [
        { label: 'Ocak', value: 1 },
        { label: 'Şubat', value: 2 },
        { label: 'Mart', value: 3 },
        { label: 'Nisan', value: 4 },
        { label: 'Mayıs', value: 5 },
        { label: 'Haziran', value: 6 },
        { label: 'Temmuz', value: 7 },
        { label: 'Ağustos', value: 8 },
        { label: 'Eylül', value: 9 },
        { label: 'Ekim', value: 10 },
        { label: 'Kasım', value: 11 },
        { label: 'Aralık', value: 12 }
    ];

    useEffect(() => {
        loadDepartmanlar();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('bordro-raporlari', 'read')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false
            });
        }
    };

    const loadDepartmanlar = async () => {
        try {
            const response = await departmanService.getAktifDepartmanlar();
            if (response.success) {
                setDepartmanlar(response.data.map(d => ({
                    label: d.ad,
                    value: d.id
                })));
            }
        } catch (error) {
            // console.error('Departmanlar yüklenirken hata:', error);
        }
    };

    const loadOzet = async () => {
        setLoading(true);
        try {
            const response = await bordroService.getOzet(filters.yil, filters.ay);
            if (response.success) {
                setOzet(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Özet yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const loadDepartmanDagilim = async () => {
        setLoading(true);
        try {
            const response = await bordroService.getDepartmanDagilim(filters.yil, filters.ay);
            if (response.success) {
                setDepartmanDagilim(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Departman dağılımı yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const generateReport = () => {
        loadOzet();
        loadDepartmanDagilim();
    };

    const exportExcel = async () => {
        if (!ozet) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen önce rapor oluşturun',
                life: 3000
            });
            return;
        }

        try {
            // Excel export için xlsx kütüphanesini kullan
            const XLSX = await import('xlsx');

            // Özet verileri
            const ozetData = [
                ['BORDRO RAPORU - ÖZET'],
                ['Dönem:', `${filters.yil} / ${ayOptions.find(a => a.value === filters.ay)?.label}`],
                [''],
                ['Toplam Personel:', ozet.toplamPersonel],
                ['Toplam Brüt Ücret:', formatCurrency(ozet.toplamBrutUcret)],
                ['Toplam Net Ücret:', formatCurrency(ozet.toplamNetUcret)],
                ['Ortalama Brüt Ücret:', formatCurrency(ozet.ortalamaBrutUcret)],
                ['Ortalama Net Ücret:', formatCurrency(ozet.ortalamaNetUcret)],
                [''],
                ['DEPARTMAN DAĞILIMI'],
                ['Departman', 'Personel Sayısı', 'Toplam Brüt', 'Toplam Net', 'Ortalama Brüt', 'Ortalama Net']
            ];

            departmanDagilim.forEach(d => {
                ozetData.push([
                    d.departmanAd || 'Diğer',
                    d.personelSayisi,
                    d.toplamBrutUcret,
                    d.toplamNetUcret,
                    d.ortalamaBrutUcret,
                    d.ortalamaNetUcret
                ]);
            });

            const ws = XLSX.utils.aoa_to_sheet(ozetData);
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Bordro Raporu');

            // Column widths
            const colWidths = [
                { wch: 30 },
                { wch: 15 },
                { wch: 15 },
                { wch: 15 },
                { wch: 15 },
                { wch: 15 }
            ];
            ws['!cols'] = colWidths;

            XLSX.writeFile(wb, `Bordro_Raporu_${filters.yil}_${filters.ay}.xlsx`);

            toast.current.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'Excel raporu başarıyla oluşturuldu',
                life: 3000
            });
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Excel raporu oluşturulurken hata oluştu',
                life: 3000
            });
        }
    };

    const exportPDF = async () => {
        if (!ozet) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen önce rapor oluşturun',
                life: 3000
            });
            return;
        }

        try {
            // jsPDF kütüphanesini kullan
            const jsPDF = (await import('jspdf')).default;
            const doc = new jsPDF();

            // Başlık
            doc.setFontSize(18);
            doc.text('BORDRO RAPORU', 105, 20, { align: 'center' });

            doc.setFontSize(12);
            doc.text(`Dönem: ${filters.yil} / ${ayOptions.find(a => a.value === filters.ay)?.label}`, 105, 30, { align: 'center' });

            // Özet bilgiler
            doc.setFontSize(14);
            doc.text('ÖZET BİLGİLER', 20, 45);

            doc.setFontSize(10);
            let yPos = 55;
            doc.text(`Toplam Personel: ${ozet.toplamPersonel}`, 20, yPos);
            yPos += 7;
            doc.text(`Toplam Brüt Ücret: ${formatCurrency(ozet.toplamBrutUcret)}`, 20, yPos);
            yPos += 7;
            doc.text(`Toplam Net Ücret: ${formatCurrency(ozet.toplamNetUcret)}`, 20, yPos);
            yPos += 7;
            doc.text(`Ortalama Brüt Ücret: ${formatCurrency(ozet.ortalamaBrutUcret)}`, 20, yPos);
            yPos += 7;
            doc.text(`Ortalama Net Ücret: ${formatCurrency(ozet.ortalamaNetUcret)}`, 20, yPos);

            // Departman dağılımı
            yPos += 15;
            doc.setFontSize(14);
            doc.text('DEPARTMAN DAĞILIMI', 20, yPos);

            yPos += 10;
            doc.setFontSize(9);

            // Tablo başlıkları
            doc.text('Departman', 20, yPos);
            doc.text('Personel', 70, yPos);
            doc.text('Toplam Brüt', 100, yPos);
            doc.text('Toplam Net', 140, yPos);

            yPos += 5;
            departmanDagilim.forEach(d => {
                if (yPos > 270) {
                    doc.addPage();
                    yPos = 20;
                }
                doc.text(d.departmanAd || 'Diğer', 20, yPos);
                doc.text(String(d.personelSayisi), 70, yPos);
                doc.text(formatCurrency(d.toplamBrutUcret), 100, yPos);
                doc.text(formatCurrency(d.toplamNetUcret), 140, yPos);
                yPos += 7;
            });

            // Footer
            const pageCount = doc.internal.getNumberOfPages();
            for (let i = 1; i <= pageCount; i++) {
                doc.setPage(i);
                doc.setFontSize(8);
                doc.text(`Sayfa ${i} / ${pageCount}`, 105, 290, { align: 'center' });
                doc.text(`Icon İK Yönetim Sistemi - ${new Date().toLocaleDateString('tr-TR')}`, 105, 295, { align: 'center' });
            }

            doc.save(`Bordro_Raporu_${filters.yil}_${filters.ay}.pdf`);

            toast.current.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'PDF raporu başarıyla oluşturuldu',
                life: 3000
            });
        } catch (error) {
            // console.error('PDF export error:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'PDF raporu oluşturulurken hata oluştu',
                life: 3000
            });
        }
    };

    const onFilterChange = (e, name) => {
        const val = (e.target && e.target.value) !== undefined ? e.target.value : e.value;
        let _filters = { ...filters };
        _filters[`${name}`] = val;
        setFilters(_filters);
    };

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(value || 0);
    };

    const currencyBodyTemplate = (rowData, field) => {
        return formatCurrency(rowData[field]);
    };

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            <Card className="p-mb-3">
                <h5>Rapor Parametreleri</h5>
                <div className="p-grid p-fluid">
                    <div className="p-col-12 p-md-3">
                        <label htmlFor="filterYil">Yıl</label>
                        <InputNumber
                            id="filterYil"
                            value={filters.yil}
                            onValueChange={(e) => onFilterChange(e, 'yil')}
                            useGrouping={false}
                        />
                    </div>

                    <div className="p-col-12 p-md-3">
                        <label htmlFor="filterAy">Ay</label>
                        <Dropdown
                            id="filterAy"
                            value={filters.ay}
                            options={ayOptions}
                            onChange={(e) => onFilterChange(e, 'ay')}
                            placeholder="Ay seçin"
                        />
                    </div>

                    <div className="p-col-12 p-md-4">
                        <label htmlFor="filterDepartman">Departman (Opsiyonel)</label>
                        <Dropdown
                            id="filterDepartman"
                            value={filters.departmanId}
                            options={departmanlar}
                            onChange={(e) => onFilterChange(e, 'departmanId')}
                            placeholder="Tüm Departmanlar"
                            showClear
                        />
                    </div>

                    <div className="p-col-12 p-md-2">
                        <label>&nbsp;</label>
                        <Button
                            label="Rapor Oluştur"
                            icon="pi pi-chart-bar"
                            onClick={generateReport}
                            loading={loading}
                        />
                    </div>
                </div>
            </Card>

            {ozet && (
                <>
                    <Card className="p-mb-3">
                        <div className="p-d-flex p-jc-between p-ai-center p-mb-3">
                            <h5>Rapor Sonuçları</h5>
                            <div>
                                <Button
                                    label="Excel İndir"
                                    icon="pi pi-file-excel"
                                    className="p-button-success p-mr-2"
                                    onClick={exportExcel}
                                />
                                <Button
                                    label="PDF İndir"
                                    icon="pi pi-file-pdf"
                                    className="p-button-danger"
                                    onClick={exportPDF}
                                />
                            </div>
                        </div>

                        <Panel header="Özet Bilgiler" className="p-mb-3">
                            <div className="p-grid">
                                <div className="p-col-12 p-md-6">
                                    <div className="p-d-flex p-jc-between p-ai-center p-mb-2">
                                        <span><strong>Toplam Personel:</strong></span>
                                        <span className="p-text-bold">{ozet.toplamPersonel}</span>
                                    </div>
                                    <Divider />
                                    <div className="p-d-flex p-jc-between p-ai-center p-mb-2">
                                        <span><strong>Toplam Brüt Ücret:</strong></span>
                                        <span className="p-text-bold">{formatCurrency(ozet.toplamBrutUcret)}</span>
                                    </div>
                                    <Divider />
                                    <div className="p-d-flex p-jc-between p-ai-center">
                                        <span><strong>Toplam Net Ücret:</strong></span>
                                        <span className="p-text-bold p-text-success">{formatCurrency(ozet.toplamNetUcret)}</span>
                                    </div>
                                </div>

                                <div className="p-col-12 p-md-6">
                                    <div className="p-d-flex p-jc-between p-ai-center p-mb-2">
                                        <span><strong>Ortalama Brüt Ücret:</strong></span>
                                        <span className="p-text-bold">{formatCurrency(ozet.ortalamaBrutUcret)}</span>
                                    </div>
                                    <Divider />
                                    <div className="p-d-flex p-jc-between p-ai-center">
                                        <span><strong>Ortalama Net Ücret:</strong></span>
                                        <span className="p-text-bold p-text-success">{formatCurrency(ozet.ortalamaNetUcret)}</span>
                                    </div>
                                </div>
                            </div>
                        </Panel>

                        {departmanDagilim.length > 0 && (
                            <Panel header="Departman Dağılımı">
                                <DataTable value={departmanDagilim} responsiveLayout="scroll">
                                    <Column
                                        field="departmanAd"
                                        header="Departman"
                                        sortable
                                    ></Column>
                                    <Column
                                        field="personelSayisi"
                                        header="Personel Sayısı"
                                        sortable
                                    ></Column>
                                    <Column
                                        field="toplamBrutUcret"
                                        header="Toplam Brüt"
                                        body={(rowData) => currencyBodyTemplate(rowData, 'toplamBrutUcret')}
                                        sortable
                                    ></Column>
                                    <Column
                                        field="toplamNetUcret"
                                        header="Toplam Net"
                                        body={(rowData) => currencyBodyTemplate(rowData, 'toplamNetUcret')}
                                        sortable
                                    ></Column>
                                    <Column
                                        field="ortalamaBrutUcret"
                                        header="Ortalama Brüt"
                                        body={(rowData) => currencyBodyTemplate(rowData, 'ortalamaBrutUcret')}
                                        sortable
                                    ></Column>
                                    <Column
                                        field="ortalamaNetUcret"
                                        header="Ortalama Net"
                                        body={(rowData) => currencyBodyTemplate(rowData, 'ortalamaNetUcret')}
                                        sortable
                                    ></Column>
                                </DataTable>
                            </Panel>
                        )}
                    </Card>
                </>
            )}

            {!ozet && (
                <Card>
                    <div className="p-text-center p-p-5">
                        <i className="pi pi-chart-bar" style={{ fontSize: '3rem', color: '#dee2e6' }}></i>
                        <h5 className="p-mt-3">Rapor Oluşturun</h5>
                        <p className="p-text-secondary">Yukarıdaki parametreleri seçerek bordro raporu oluşturabilirsiniz.</p>
                    </div>
                </Card>
            )}
        </div>
    );
};

export default BordroRaporlari;
