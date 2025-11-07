import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { Calendar } from 'primereact/calendar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Badge } from 'primereact/badge';
import { Toolbar } from 'primereact/toolbar';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { TabView, TabPanel } from 'primereact/tabview';
import { Sidebar } from 'primereact/sidebar';
import StatisticsChart from '../components/StatisticsChart';
import StatisticsCard from '../components/StatisticsCard';
import { Card } from 'primereact/card';
import { Divider } from 'primereact/divider';
import { Rating } from 'primereact/rating';
import { ProgressBar } from 'primereact/progressbar';
import { Timeline } from 'primereact/timeline';
import { FilterMatchMode } from 'primereact/api';
import { classNames } from 'primereact/utils';
import basvuruService from '../services/basvuruService';

const BasvuruYonetimi = () => {
    const [basvurular, setBasvurular] = useState([]);
    const [filteredBasvurular, setFilteredBasvurular] = useState([]);
    const [isIlanlari, setIsIlanlari] = useState([]);
    const [adaylar, setAdaylar] = useState([]);
    const [loading, setLoading] = useState(true);
    const [basvuruDialog, setBasvuruDialog] = useState(false);
    const [basvuru, setBasvuru] = useState({});
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState('');
    const [selectedBasvuru, setSelectedBasvuru] = useState(null);
    const [detayDialog, setDetayDialog] = useState(false);
    const [puanDialog, setPuanDialog] = useState(false);
    const [durumDialog, setDurumDialog] = useState(false);
    const [mulakatDialog, setMulakatDialog] = useState(false);
    const [teklifDialog, setTeklifDialog] = useState(false);
    const [puan, setPuan] = useState(0);
    const [puanNotu, setPuanNotu] = useState('');
    const [yeniDurum, setYeniDurum] = useState('');
    const [durumNotu, setDurumNotu] = useState('');
    const [mulakat, setMulakat] = useState({});
    const [teklif, setTeklif] = useState({});
    const [filters, setFilters] = useState({
        'global': { value: null, matchMode: FilterMatchMode.CONTAINS },
        'adayAd': { value: null, matchMode: FilterMatchMode.CONTAINS },
        'ilanBaslik': { value: null, matchMode: FilterMatchMode.CONTAINS },
        'durum': { value: null, matchMode: FilterMatchMode.EQUALS }
    });
    const [istatistikDialog, setIstatistikDialog] = useState(false);
    const [istatistikActiveTab, setIstatistikActiveTab] = useState(0);
    const [dateRange, setDateRange] = useState({ start: null, end: null });
    const [istatistikler, setIstatistikler] = useState({});
    const [istatistikLoading, setIstatistikLoading] = useState(false);
    const [aramaDialog, setAramaDialog] = useState(false);
    const [aramaKriterleri, setAramaKriterleri] = useState({});
    const toast = useRef(null);
    const dt = useRef(null);

    useEffect(() => {
        loadBasvurular();
        loadIsIlanlari();
        loadAdaylar();
        loadIstatistikler();
    }, []);

    const loadBasvurular = async () => {
        try {
            const response = await basvuruService.getAll();
            if (response.success) {
                setBasvurular(response.data);
                setFilteredBasvurular(response.data);
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Başvurular yüklenirken hata oluştu.' });
        } finally {
            setLoading(false);
        }
    };

    const loadIsIlanlari = async () => {
        try {
            const response = await basvuruService.getAktifIsIlanlari();
            if (response.success) {
                setIsIlanlari(response.data);
            }
        } catch (error) {
            // console.error('İş ilanları yüklenirken hata:', error);
        }
    };

    const loadAdaylar = async () => {
        try {
            const response = await basvuruService.getAktifAdaylar();
            if (response.success) {
                setAdaylar(response.data);
            }
        } catch (error) {
            // console.error('Adaylar yüklenirken hata:', error);
        }
    };

    const loadIstatistikler = async () => {
        try {
            setIstatistikLoading(true);
            const response = await basvuruService.getIstatistikler();
            if (response.success) {
                setIstatistikler(response.data);
            }
        } catch (error) {
            // console.error('İstatistikler yüklenirken hata:', error);
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'İstatistikler yüklenirken hata oluştu.' });
        } finally {
            setIstatistikLoading(false);
        }
    };

    const openNew = () => {
        setBasvuru({
            ilanId: null,
            adayId: null,
            basvuruTarihi: new Date(),
            durum: 'YeniBasvuru',
            kapakMektubu: '',
            aktif: true
        });
        setSubmitted(false);
        setBasvuruDialog(true);
    };

    const editBasvuru = (basvuru) => {
        setBasvuru({
            ...basvuru,
            basvuruTarihi: new Date(basvuru.basvuruTarihi)
        });
        setBasvuruDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setBasvuruDialog(false);
        setPuanDialog(false);
        setDurumDialog(false);
        setMulakatDialog(false);
        setTeklifDialog(false);
        setAramaDialog(false);
    };

    const hideDetailDialog = () => {
        setDetayDialog(false);
        setSelectedBasvuru(null);
    };

    const saveBasvuru = async () => {
        setSubmitted(true);

        if (basvuru.ilanId && basvuru.adayId) {
            try {
                let response;
                if (basvuru.id) {
                    response = await basvuruService.update(basvuru.id, basvuru);
                } else {
                    response = await basvuruService.create(basvuru);
                }

                if (response.success) {
                    toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Başvuru başarıyla kaydedildi.' });
                    loadBasvurular();
                    loadIstatistikler();
                    hideDialog();
                } else {
                    toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
                }
            } catch (error) {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Başvuru kaydedilirken hata oluştu.' });
            }
        }
    };

    const confirmDeleteBasvuru = (basvuru) => {
        confirmDialog({
            message: `${basvuru.adayAd} adayının başvurusunu silmek istediğinizden emin misiniz?`,
            header: 'Başvuru Sil',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'Evet',
            rejectLabel: 'Hayır',
            accept: () => deleteBasvuru(basvuru)
        });
    };

    const deleteBasvuru = async (basvuru) => {
        try {
            const response = await basvuruService.delete(basvuru.id);
            if (response.success) {
                setBasvurular(basvurular.filter(val => val.id !== basvuru.id));
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Başvuru başarıyla silindi.' });
                loadIstatistikler();
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Başvuru silinirken hata oluştu.' });
        }
    };

    const showDetail = (basvuru) => {
        setSelectedBasvuru(basvuru);
        setDetayDialog(true);
    };

    const showPuanDialog = (basvuru) => {
        setSelectedBasvuru(basvuru);
        setPuan(basvuru.puan || 0);
        setPuanNotu(basvuru.puanNotu || '');
        setPuanDialog(true);
    };

    const savePuan = async () => {
        try {
            const puanData = {
                puan: puan,
                puanNotu: puanNotu
            };

            const response = await basvuruService.puanVer(selectedBasvuru.id, puanData);
            if (response.success) {
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Puan başarıyla verildi.' });
                loadBasvurular();
                hideDialog();
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Puan verilirken hata oluştu.' });
        }
    };

    const showDurumDialog = (basvuru) => {
        setSelectedBasvuru(basvuru);
        setYeniDurum(basvuru.durum);
        setDurumNotu('');
        setDurumDialog(true);
    };

    const saveDurum = async () => {
        try {
            const durumData = {
                durum: yeniDurum,
                durumNotu: durumNotu
            };

            const response = await basvuruService.updateDurum(selectedBasvuru.id, durumData);
            if (response.success) {
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Durum başarıyla güncellendi.' });
                loadBasvurular();
                loadIstatistikler();
                hideDialog();
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Durum güncellenirken hata oluştu.' });
        }
    };

    const formatDate = (value) => {
        if (!value) return '';
        return new Date(value).toLocaleDateString('tr-TR');
    };

    const durumBodyTemplate = (rowData) => {
        const severity = basvuruService.getDurumSeviyesi(rowData.durum);
        return <Badge value={rowData.durumText} severity={severity}></Badge>;
    };

    const puanBodyTemplate = (rowData) => {
        if (!rowData.puan) return '-';
        const color = basvuruService.getPuanColor(rowData.puan);
        return (
            <div className="flex align-items-center gap-2">
                <Badge value={rowData.puan} style={{ backgroundColor: color }}></Badge>
                <Rating value={Math.round(rowData.puan / 20)} readOnly stars={5} cancel={false} />
            </div>
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button icon="pi pi-eye" className="p-button-rounded p-button-info p-button-sm"
                        onClick={() => showDetail(rowData)} tooltip="Detay Görüntüle" />
                <Button icon="pi pi-star" className="p-button-rounded p-button-warning p-button-sm"
                        onClick={() => showPuanDialog(rowData)} tooltip="Puan Ver" />
                <Button icon="pi pi-refresh" className="p-button-rounded p-button-secondary p-button-sm"
                        onClick={() => showDurumDialog(rowData)} tooltip="Durum Güncelle" />
                <Button icon="pi pi-pencil" className="p-button-rounded p-button-success p-button-sm"
                        onClick={() => editBasvuru(rowData)} tooltip="Düzenle" />
                <Button icon="pi pi-trash" className="p-button-rounded p-button-danger p-button-sm"
                        onClick={() => confirmDeleteBasvuru(rowData)} tooltip="Sil" />
            </div>
        );
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex gap-2">
                <Button label="Yeni Başvuru" icon="pi pi-plus" className="p-button-success" onClick={openNew} />
                <Button label="İstatistikler" icon="pi pi-chart-bar" className="p-button-info" onClick={() => setIstatistikDialog(true)} />
                <Button label="Gelişmiş Arama" icon="pi pi-search" className="p-button-help" onClick={() => setAramaDialog(true)} />
                <Button label="CSV" icon="pi pi-file" className="p-button-secondary" onClick={exportCSV} tooltip="CSV olarak dışa aktar" />
                <Button label="Excel" icon="pi pi-file-excel" className="p-button-secondary" onClick={exportExcel} tooltip="Excel olarak dışa aktar" />
                <Button label="PDF" icon="pi pi-file-pdf" className="p-button-secondary" onClick={exportPdf} tooltip="PDF olarak dışa aktar" />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <div className="flex gap-2">
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText value={globalFilter} onChange={(e) => setGlobalFilter(e.target.value)} placeholder="Arama..." />
                </span>
            </div>
        );
    };

    const basvuruDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveBasvuru} />
        </div>
    );

    const puanDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={savePuan} />
        </div>
    );

    const durumDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Güncelle" icon="pi pi-check" className="p-button-text" onClick={saveDurum} />
        </div>
    );

    const getIstatistikCards = () => {
        const cards = [
            { title: 'Toplam Başvuru', value: istatistikler.toplamBasvuru || 0, previousValue: 0, icon: 'pi-users', color: 'blue' },
            { title: 'Yeni Başvurular', value: istatistikler.yeniBasvuru || 0, previousValue: 0, icon: 'pi-plus-circle', color: 'green' },
            { title: 'Değerlendiriliyor', value: istatistikler.degerlendiriliyor || 0, previousValue: 0, icon: 'pi-clock', color: 'orange' },
            { title: 'Mülakat Bekliyor', value: istatistikler.mulakatBekleniyor || 0, previousValue: 0, icon: 'pi-calendar', color: 'purple' },
            { title: 'İşe Alındı', value: istatistikler.iseAlindi || 0, previousValue: 0, icon: 'pi-check-circle', color: 'teal' },
            { title: 'Reddedildi', value: istatistikler.reddedildi || 0, previousValue: 0, icon: 'pi-times-circle', color: 'red' }
        ];

        return cards.map((card, index) => (
            <div key={index} className="col-12 md:col-6 lg:col-4">
                <StatisticsCard
                    title={card.title}
                    value={card.value}
                    previousValue={card.previousValue}
                    icon={card.icon}
                    color={card.color}
                    showTrend={true}
                    animate={true}
                />
            </div>
        ));
    };

    const getDurumChartData = () => {
        return {
            labels: ['Yeni Başvuru', 'Değerlendiriliyor', 'Mülakat Bekliyor', 'Mülakat Tamamlandı', 'Teklif Verildi', 'İşe Alındı', 'Reddedildi', 'Aday Vazgeçti'],
            datasets: [
                {
                    data: [
                        istatistikler.yeniBasvuru || 0,
                        istatistikler.degerlendiriliyor || 0,
                        istatistikler.mulakatBekleniyor || 0,
                        istatistikler.mulakatTamamlandi || 0,
                        istatistikler.teklifVerildi || 0,
                        istatistikler.iseAlindi || 0,
                        istatistikler.reddedildi || 0,
                        istatistikler.adayVazgecti || 0
                    ],
                    backgroundColor: [
                        '#3B82F6', // Blue
                        '#F59E0B', // Orange
                        '#8B5CF6', // Purple
                        '#06B6D4', // Cyan
                        '#84CC16', // Lime
                        '#10B981', // Teal
                        '#EF4444', // Red
                        '#6B7280'  // Gray
                    ],
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }
            ]
        };
    };

    const getOranChartData = () => {
        return {
            labels: ['İşe Alım Oranı', 'Red Oranı', 'Bekleyen'],
            datasets: [
                {
                    data: [
                        istatistikler.iseAlimOrani || 0,
                        istatistikler.redOrani || 0,
                        100 - (istatistikler.iseAlimOrani || 0) - (istatistikler.redOrani || 0)
                    ],
                    backgroundColor: [
                        '#10B981', // Green
                        '#EF4444', // Red
                        '#6B7280'  // Gray
                    ],
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }
            ]
        };
    };

    const getBasvuruTimeline = (basvuru) => {
        const events = [
            {
                status: 'Başvuru',
                date: formatDate(basvuru.basvuruTarihi),
                icon: 'pi pi-user-plus',
                color: '#9C27B0'
            }
        ];

        // Diğer durumlar için tarihler eklenebilir
        if (basvuru.durum !== 'YeniBasvuru') {
            events.push({
                status: basvuru.durumAdi,
                date: formatDate(new Date()),
                icon: 'pi pi-check',
                color: '#607D8B'
            });
        }

        return events;
    };

    const exportCSV = () => {
        dt.current.exportCSV();
    };

    const exportPdf = () => {
        import('jspdf').then((jsPDF) => {
            import('jspdf-autotable').then(() => {
                const doc = new jsPDF.default(0, 0);
                doc.autoTable({
                    head: [['Aday', 'İş İlanı', 'Başvuru Tarihi', 'Durum', 'Puan']],
                    body: basvurular.map(basvuru => [
                        basvuru.adayAd,
                        basvuru.ilanBaslik,
                        formatDate(basvuru.basvuruTarihi),
                        basvuru.durumText,
                        basvuru.puan || '-'
                    ])
                });
                doc.save('basvuru-listesi.pdf');
            });
        });
    };

    const exportExcel = () => {
        import('xlsx').then((xlsx) => {
            const worksheet = xlsx.utils.json_to_sheet(basvurular.map(basvuru => ({
                'Aday': basvuru.adayAd,
                'İş İlanı': basvuru.ilanBaslik,
                'Başvuru Tarihi': formatDate(basvuru.basvuruTarihi),
                'Durum': basvuru.durumText,
                'Puan': basvuru.puan || '-',
                'Beklenen Maaş': basvuru.beklenenMaas || '-',
                'İşe Başlama Tarihi': basvuru.iseBaslamaTarihi ? formatDate(basvuru.iseBaslamaTarihi) : '-'
            })));
            const workbook = { Sheets: { 'Başvurular': worksheet }, SheetNames: ['Başvurular'] };
            const excelBuffer = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
            saveAsExcelFile(excelBuffer, 'basvuru-listesi');
        });
    };

    const saveAsExcelFile = (buffer, fileName) => {
        import('file-saver').then((module) => {
            if (module && module.default) {
                let EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
                let EXCEL_EXTENSION = '.xlsx';
                const data = new Blob([buffer], { type: EXCEL_TYPE });
                module.default.saveAs(data, fileName + '_export_' + new Date().getTime() + EXCEL_EXTENSION);
            }
        });
    };

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            <div className="card">
                <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate}></Toolbar>

                <DataTable
                    ref={dt}
                    value={basvurular}
                    selection={selectedBasvuru}
                    onSelectionChange={(e) => setSelectedBasvuru(e.value)}
                    dataKey="id"
                    paginator
                    rows={10}
                    rowsPerPageOptions={[5, 10, 25]}
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    currentPageReportTemplate="{first} - {last} / {totalRecords} başvuru"
                    globalFilter={globalFilter}
                    emptyMessage="Başvuru bulunamadı."
                    loading={loading}
                    filters={filters}
                    filterDisplay="menu"
                    responsiveLayout="scroll">

                    <Column field="adayAd" header="Aday" sortable filter filterPlaceholder="Aday ara" style={{ minWidth: '12rem' }} />
                    <Column field="ilanBaslik" header="İş İlanı" sortable filter filterPlaceholder="İlan ara" style={{ minWidth: '14rem' }} />
                    <Column field="basvuruTarihi" header="Başvuru Tarihi" sortable body={(rowData) => formatDate(rowData.basvuruTarihi)} style={{ minWidth: '10rem' }} />
                    <Column field="durum" header="Durum" body={durumBodyTemplate} sortable filter filterElement={
                        <Dropdown value={filters['durum'].value} options={basvuruService.getDurumListesi()}
                                  onChange={(e) => setFilters({...filters, durum: {...filters['durum'], value: e.value}})}
                                  placeholder="Durum seçin" className="p-column-filter" showClear optionLabel="label" optionValue="value" />
                    } style={{ minWidth: '12rem' }} />
                    <Column field="puan" header="Puan" body={puanBodyTemplate} sortable style={{ minWidth: '10rem' }} />
                    <Column body={actionBodyTemplate} exportable={false} style={{ minWidth: '12rem' }}></Column>
                </DataTable>
            </div>

            <Dialog visible={basvuruDialog} style={{ width: '450px' }} header="Başvuru Detayları" modal className="p-fluid" footer={basvuruDialogFooter} onHide={hideDialog}>
                <div className="field">
                    <label htmlFor="ilanId">İş İlanı</label>
                    <Dropdown id="ilanId" value={basvuru.ilanId} onChange={(e) => setBasvuru({...basvuru, ilanId: e.value})}
                              options={isIlanlari} optionLabel="baslik" optionValue="id" placeholder="İş İlanı Seçin"
                              className={submitted && !basvuru.ilanId ? 'p-invalid' : ''} />
                    {submitted && !basvuru.ilanId && <small className="p-error">İş İlanı gereklidir.</small>}
                </div>

                <div className="field">
                    <label htmlFor="adayId">Aday</label>
                    <Dropdown id="adayId" value={basvuru.adayId} onChange={(e) => setBasvuru({...basvuru, adayId: e.value})}
                              options={adaylar} optionLabel="adSoyad" optionValue="id" placeholder="Aday Seçin"
                              className={submitted && !basvuru.adayId ? 'p-invalid' : ''} />
                    {submitted && !basvuru.adayId && <small className="p-error">Aday gereklidir.</small>}
                </div>

                <div className="field">
                    <label htmlFor="basvuruTarihi">Başvuru Tarihi</label>
                    <Calendar id="basvuruTarihi" value={basvuru.basvuruTarihi} onChange={(e) => setBasvuru({...basvuru, basvuruTarihi: e.value})}
                              dateFormat="dd.mm.yy" showIcon locale="tr" />
                </div>

                <div className="field">
                    <label htmlFor="durum">Durum</label>
                    <Dropdown id="durum" value={basvuru.durum} onChange={(e) => setBasvuru({...basvuru, durum: e.value})}
                              options={basvuruService.getDurumListesi()} optionLabel="label" optionValue="value" placeholder="Durum Seçin" />
                </div>

                <div className="field">
                    <label htmlFor="kapakMektubu">Kapak Mektubu</label>
                    <InputTextarea id="kapakMektubu" value={basvuru.kapakMektubu} onChange={(e) => setBasvuru({...basvuru, kapakMektubu: e.target.value})}
                                   rows={3} cols={20} />
                </div>
            </Dialog>

            <Dialog visible={detayDialog} style={{ width: '80vw', height: '80vh' }}
                header={
                    <div className="flex justify-content-between align-items-center w-full">
                        <span>Başvuru Detayı</span>
                        {selectedBasvuru && (
                            <div className="flex gap-2">
                                <Button
                                    icon="pi pi-file-pdf"
                                    className="p-button-rounded p-button-info p-button-sm"
                                    tooltip="CV Görüntüle"
                                    onClick={() => window.open(`/api/aday/${selectedBasvuru.adayId}/cv-goruntule`, '_blank')}
                                />
                                <Button
                                    icon="pi pi-star"
                                    className="p-button-rounded p-button-warning p-button-sm"
                                    tooltip="Puan Ver"
                                    onClick={() => showPuanDialog(selectedBasvuru)}
                                />
                                <Button
                                    icon="pi pi-refresh"
                                    className="p-button-rounded p-button-secondary p-button-sm"
                                    tooltip="Durum Güncelle"
                                    onClick={() => showDurumDialog(selectedBasvuru)}
                                />
                                <Button
                                    icon="pi pi-calendar"
                                    className="p-button-rounded p-button-help p-button-sm"
                                    tooltip="Mülakat Planla"
                                    onClick={() => toast.current?.show({ severity: 'info', summary: 'Bilgi', detail: 'Mülakat planlama özelliği geliştiriliyor...' })}
                                />
                            </div>
                        )}
                    </div>
                }
                modal onHide={hideDetailDialog} maximizable>
                {selectedBasvuru && (
                    <TabView>
                        <TabPanel header="Genel Bilgiler">
                            <div className="grid">
                                <div className="col-12 md:col-6">
                                    <Card title="Aday Bilgileri">
                                        <p><strong>Ad Soyad:</strong> {selectedBasvuru.adayAd}</p>
                                        <p><strong>Başvuru Tarihi:</strong> {formatDate(selectedBasvuru.basvuruTarihi)}</p>
                                        <p><strong>Durum:</strong> <Badge value={selectedBasvuru.durumText} severity={basvuruService.getDurumSeviyesi(selectedBasvuru.durum)} /></p>
                                        <p><strong>Puan:</strong> {selectedBasvuru.puan ? <Badge value={selectedBasvuru.puan} style={{backgroundColor: basvuruService.getPuanColor(selectedBasvuru.puan)}} /> : 'Verilmedi'}</p>
                                    </Card>
                                </div>
                                <div className="col-12 md:col-6">
                                    <Card title="İş İlanı Bilgileri">
                                        <p><strong>İlan Başlığı:</strong> {selectedBasvuru.ilanBaslik}</p>
                                        <p><strong>Pozisyon:</strong> {selectedBasvuru.pozisyon || '-'}</p>
                                        <p><strong>Beklenen Maaş:</strong> {selectedBasvuru.beklenenMaas ? basvuruService.formatCurrency(selectedBasvuru.beklenenMaas) : '-'}</p>
                                        <p><strong>Çalışma Şekli:</strong> {selectedBasvuru.calismaSekli || '-'}</p>
                                    </Card>
                                </div>
                            </div>

                            {selectedBasvuru.kapakMektubu && (
                                <div className="mt-4">
                                    <Card title="Kapak Mektubu">
                                        <p>{selectedBasvuru.kapakMektubu}</p>
                                    </Card>
                                </div>
                            )}
                        </TabPanel>

                        <TabPanel header="Süreç Takibi">
                            <div className="grid">
                                <div className="col-12 md:col-6">
                                    <Card title="Başvuru Süreci">
                                        <Timeline value={getBasvuruTimeline(selectedBasvuru)} align="left" className="customized-timeline"
                                                  marker={(item) => <span className="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1" style={{ backgroundColor: item.color }}>
                                                      <i className={item.icon}></i>
                                                  </span>}
                                                  content={(item) => (
                                                      <div>
                                                          <div className="font-medium">{item.status}</div>
                                                          <div className="text-600">{item.date}</div>
                                                      </div>
                                                  )} />
                                    </Card>
                                </div>
                                <div className="col-12 md:col-6">
                                    <Card title="Değerlendirme">
                                        <div className="mb-3">
                                            <label>Puan:</label>
                                            <div className="mt-2">
                                                {selectedBasvuru.puan ? (
                                                    <div className="flex align-items-center gap-2">
                                                        <ProgressBar value={selectedBasvuru.puan} style={{width: '200px'}} />
                                                        <Badge value={selectedBasvuru.puan} style={{backgroundColor: basvuruService.getPuanColor(selectedBasvuru.puan)}} />
                                                    </div>
                                                ) : (
                                                    <span className="text-500">Henüz puan verilmedi</span>
                                                )}
                                            </div>
                                        </div>
                                        {selectedBasvuru.puanNotu && (
                                            <div>
                                                <label>Puan Notu:</label>
                                                <p className="mt-1">{selectedBasvuru.puanNotu}</p>
                                            </div>
                                        )}
                                    </Card>
                                </div>
                            </div>
                        </TabPanel>
                    </TabView>
                )}
            </Dialog>

            <Dialog visible={puanDialog} style={{ width: '400px' }} header="Puan Ver" modal footer={puanDialogFooter} onHide={hideDialog}>
                <div className="field">
                    <label htmlFor="puan">Puan (0-100)</label>
                    <div className="flex align-items-center gap-3 mt-2">
                        <Rating value={Math.round(puan / 20)} onChange={(e) => setPuan(e.value * 20)} stars={5} cancel={false} />
                        <InputText id="puan" value={puan} onChange={(e) => setPuan(Math.min(100, Math.max(0, parseInt(e.target.value) || 0)))}
                                   type="number" min="0" max="100" className="w-4rem" />
                    </div>
                </div>
                <div className="field">
                    <label htmlFor="puanNotu">Puan Notu</label>
                    <InputTextarea id="puanNotu" value={puanNotu} onChange={(e) => setPuanNotu(e.target.value)}
                                   rows={3} cols={20} placeholder="Puanlama ile ilgili notlarınızı yazın..." />
                </div>
            </Dialog>

            <Dialog visible={durumDialog} style={{ width: '400px' }} header="Durum Güncelle" modal footer={durumDialogFooter} onHide={hideDialog}>
                <div className="field">
                    <label htmlFor="yeniDurum">Yeni Durum</label>
                    <Dropdown id="yeniDurum" value={yeniDurum} onChange={(e) => setYeniDurum(e.value)}
                              options={basvuruService.getDurumListesi()} optionLabel="label" optionValue="value"
                              placeholder="Yeni Durum Seçin" className="w-full" />
                </div>
                <div className="field">
                    <label htmlFor="durumNotu">Durum Notu</label>
                    <InputTextarea id="durumNotu" value={durumNotu} onChange={(e) => setDurumNotu(e.target.value)}
                                   rows={3} cols={20} placeholder="Durum değişikliği ile ilgili notlarınızı yazın..." />
                </div>
            </Dialog>

            <Dialog visible={istatistikDialog} style={{ width: '90vw', height: '85vh' }}
                header={
                    <div className="flex justify-content-between align-items-center w-full">
                        <span className="text-2xl font-bold">📊 Başvuru İstatistikleri</span>
                        <div className="flex gap-2">
                            <Button
                                icon="pi pi-refresh"
                                className="p-button-rounded p-button-text"
                                onClick={loadIstatistikler}
                                loading={istatistikLoading}
                                tooltip="Yenile"
                            />
                            <Button
                                icon="pi pi-download"
                                className="p-button-rounded p-button-text"
                                tooltip="Excel İndir"
                                onClick={() => toast.current?.show({ severity: 'info', summary: 'Bilgi', detail: 'Excel indirme özelliği geliştiriliyor...' })}
                            />
                        </div>
                    </div>
                }
                modal onHide={() => setIstatistikDialog(false)} maximizable>

                {istatistikLoading ? (
                    <div className="text-center py-8">
                        <i className="pi pi-spin pi-spinner text-4xl text-primary"></i>
                        <p className="mt-3 text-lg">İstatistikler yükleniyor...</p>
                    </div>
                ) : (
                    <TabView activeIndex={istatistikActiveTab} onTabChange={(e) => setIstatistikActiveTab(e.index)}>
                        <TabPanel header="📈 Genel Bakış">
                            <div className="grid mt-3">
                                {getIstatistikCards()}
                            </div>

                            <Divider />

                            <div className="grid">
                                <div className="col-12 md:col-6">
                                    <Card title="Başarı Oranları" className="h-full">
                                        <div className="mb-3">
                                            <div className="flex justify-content-between align-items-center mb-2">
                                                <span className="font-medium">İşe Alım Oranı</span>
                                                <Badge value={`${istatistikler.iseAlimOrani || 0}%`} severity="success" />
                                            </div>
                                            <ProgressBar value={istatistikler.iseAlimOrani || 0} className="mb-3" />
                                        </div>
                                        <div className="mb-3">
                                            <div className="flex justify-content-between align-items-center mb-2">
                                                <span className="font-medium">Red Oranı</span>
                                                <Badge value={`${istatistikler.redOrani || 0}%`} severity="danger" />
                                            </div>
                                            <ProgressBar value={istatistikler.redOrani || 0} className="mb-3" />
                                        </div>
                                        <div className="text-center mt-4">
                                            <h4 className="text-primary">Toplam Değerlendirme</h4>
                                            <div className="text-3xl font-bold text-900">
                                                {((istatistikler.iseAlimOrani || 0) + (istatistikler.redOrani || 0)).toFixed(1)}%
                                            </div>
                                            <small className="text-500">İşlem Tamamlanan Başvurular</small>
                                        </div>
                                    </Card>
                                </div>
                                <div className="col-12 md:col-6">
                                    <Card title="Top Performans Metrikleri" className="h-full">
                                        <div className="flex flex-column gap-3">
                                            <div className="flex justify-content-between align-items-center p-3 border-1 border-200 border-round">
                                                <div>
                                                    <div className="font-medium text-900">Ortalama İşlem Süresi</div>
                                                    <small className="text-600">Başvuru → Karar</small>
                                                </div>
                                                <div className="text-right">
                                                    <div className="text-2xl font-bold text-orange-600">12</div>
                                                    <small className="text-500">gün</small>
                                                </div>
                                            </div>
                                            <div className="flex justify-content-between align-items-center p-3 border-1 border-200 border-round">
                                                <div>
                                                    <div className="font-medium text-900">En Çok Başvuru</div>
                                                    <small className="text-600">Bu ay</small>
                                                </div>
                                                <div className="text-right">
                                                    <div className="text-2xl font-bold text-blue-600">{istatistikler.yeniBasvuru || 0}</div>
                                                    <small className="text-500">başvuru</small>
                                                </div>
                                            </div>
                                            <div className="flex justify-content-between align-items-center p-3 border-1 border-200 border-round">
                                                <div>
                                                    <div className="font-medium text-900">Aktif Süreçler</div>
                                                    <small className="text-600">Devam eden</small>
                                                </div>
                                                <div className="text-right">
                                                    <div className="text-2xl font-bold text-purple-600">
                                                        {(istatistikler.degerlendiriliyor || 0) + (istatistikler.mulakatBekleniyor || 0)}
                                                    </div>
                                                    <small className="text-500">süreç</small>
                                                </div>
                                            </div>
                                        </div>
                                    </Card>
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel header="📊 Grafikler">
                            <div className="grid mt-3">
                                <div className="col-12 md:col-6">
                                    <Card title="Başvuru Durumu Dağılımı" className="h-full">
                                        <StatisticsChart
                                            type="doughnut"
                                            data={getDurumChartData()}
                                            title="Başvuru Durumları"
                                            height={350}
                                        />
                                    </Card>
                                </div>
                                <div className="col-12 md:col-6">
                                    <Card title="Başarı Oranları" className="h-full">
                                        <StatisticsChart
                                            type="pie"
                                            data={getOranChartData()}
                                            title="İşe Alım vs Red Oranları"
                                            height={350}
                                        />
                                    </Card>
                                </div>
                                <div className="col-12">
                                    <Card title="Aylık Başvuru Trendi" className="h-full">
                                        <StatisticsChart
                                            type="bar"
                                            data={(() => {
                                                // Get monthly trends data with proper null checking
                                                const trendData = Array.isArray(istatistikler.aylikTrendler) ? istatistikler.aylikTrendler : [];

                                                // Generate current month names for last 6 months if no data
                                                const generateCurrentMonths = () => {
                                                    const months = [];
                                                    const now = new Date();
                                                    for (let i = 5; i >= 0; i--) {
                                                        const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
                                                        months.push({
                                                            tarih: `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}`,
                                                            toplam: 0,
                                                            iseAlindi: 0
                                                        });
                                                    }
                                                    return months;
                                                };

                                                const dataToUse = trendData.length > 0 ? trendData : generateCurrentMonths();

                                                return {
                                                    labels: dataToUse.map(t => {
                                                        if (!t || !t.tarih) return '';
                                                        try {
                                                            const [yil, ay] = t.tarih.split('-');
                                                            const ayAdlari = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];
                                                            const monthIndex = parseInt(ay) - 1;
                                                            return monthIndex >= 0 && monthIndex < 12 ? `${ayAdlari[monthIndex]} ${yil}` : '';
                                                        } catch (error) {
            // console.error('Date parsing error:', error);
                                                            return '';
                                                        }
                                                    }).filter(label => label !== ''),
                                                    datasets: [
                                                        {
                                                            label: 'Toplam Başvuru',
                                                            data: dataToUse.map(t => t && typeof t.toplam === 'number' ? t.toplam : 0),
                                                            backgroundColor: 'rgba(59, 130, 246, 0.8)',
                                                            borderColor: 'rgba(59, 130, 246, 1)',
                                                            borderWidth: 2
                                                        },
                                                        {
                                                            label: 'İşe Alınan',
                                                            data: dataToUse.map(t => t && typeof t.iseAlindi === 'number' ? t.iseAlindi : 0),
                                                            backgroundColor: 'rgba(16, 185, 129, 0.8)',
                                                            borderColor: 'rgba(16, 185, 129, 1)',
                                                            borderWidth: 2
                                                        }
                                                    ]
                                                };
                                            })()}
                                            title="Son 6 Aylık Trend"
                                            height={300}
                                        />
                                    </Card>
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel header="🎯 Detaylı Analiz">
                            <div className="grid mt-3">
                                <div className="col-12">
                                    <Card title="Gelişmiş İstatistikler" className="mb-4">
                                        <div className="text-center mb-4">
                                            <h3 className="text-primary mb-2">📊 Detaylı Performans Raporu</h3>
                                            <p className="text-600">Bu bölümde daha detaylı analizler ve filtreli raporlar yer alacak</p>
                                        </div>

                                        <div className="grid">
                                            <div className="col-12 md:col-4">
                                                <div className="text-center p-4 border-1 border-200 border-round">
                                                    <i className="pi pi-clock text-4xl text-orange-500 mb-3"></i>
                                                    <h4 className="mt-0 mb-2">Ortalama Süre</h4>
                                                    <div className="text-2xl font-bold text-900">8.5 gün</div>
                                                    <small className="text-600">Başvuru işlem süresi</small>
                                                </div>
                                            </div>
                                            <div className="col-12 md:col-4">
                                                <div className="text-center p-4 border-1 border-200 border-round">
                                                    <i className="pi pi-star text-4xl text-yellow-500 mb-3"></i>
                                                    <h4 className="mt-0 mb-2">Ortalama Puan</h4>
                                                    <div className="text-2xl font-bold text-900">76.2</div>
                                                    <small className="text-600">Değerlendirme puanı</small>
                                                </div>
                                            </div>
                                            <div className="col-12 md:col-4">
                                                <div className="text-center p-4 border-1 border-200 border-round">
                                                    <i className="pi pi-users text-4xl text-blue-500 mb-3"></i>
                                                    <h4 className="mt-0 mb-2">Aktif Adaylar</h4>
                                                    <div className="text-2xl font-bold text-900">
                                                        {(istatistikler.degerlendiriliyor || 0) + (istatistikler.mulakatBekleniyor || 0)}
                                                    </div>
                                                    <small className="text-600">Süreçte olan</small>
                                                </div>
                                            </div>
                                        </div>

                                        <Divider />

                                        <div className="text-center">
                                            <h4 className="text-600">🔄 Gelişmiş Filtreleme</h4>
                                            <p className="text-500 mb-4">Tarih aralığı, departman ve pozisyon bazlı filtreleme özellikleri yakında eklenecek</p>
                                            <div className="flex gap-2 justify-content-center">
                                                <Button label="Tarih Filtresi" icon="pi pi-calendar" className="p-button-outlined" disabled />
                                                <Button label="Departman Filtresi" icon="pi pi-building" className="p-button-outlined" disabled />
                                                <Button label="Pozisyon Filtresi" icon="pi pi-briefcase" className="p-button-outlined" disabled />
                                            </div>
                                        </div>
                                    </Card>
                                </div>
                            </div>
                        </TabPanel>
                    </TabView>
                )}
            </Dialog>

            <ConfirmDialog />
        </div>
    );
};

export default BasvuruYonetimi;