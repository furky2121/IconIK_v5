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
import { Card } from 'primereact/card';
import { Rating } from 'primereact/rating';
import { FilterMatchMode } from 'primereact/api';
import iseAlimService from '../services/iseAlimService';
import personelService from '../services/personelService';

const MulakatTakvimi = () => {
    const [mulakatlar, setMulakatlar] = useState([]);
    const [basvurular, setBasvurular] = useState([]);
    const [personeller, setPersoneller] = useState([]);
    const [loading, setLoading] = useState(true);
    const [mulakatDialog, setMulakatDialog] = useState(false);
    const [deleteDialog, setDeleteDialog] = useState(false);
    const [sonucDialog, setSonucDialog] = useState(false);
    const [mulakat, setMulakat] = useState({});
    const [selectedMulakat, setSelectedMulakat] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState('');
    const [filters, setFilters] = useState({
        'global': { value: null, matchMode: FilterMatchMode.CONTAINS },
        'adayAd': { value: null, matchMode: FilterMatchMode.CONTAINS },
        'ilanBaslik': { value: null, matchMode: FilterMatchMode.CONTAINS },
        'durum': { value: null, matchMode: FilterMatchMode.EQUALS }
    });

    const toast = useRef(null);
    const dt = useRef(null);

    // Mülakat türleri
    const mulakatTurleri = [
        { label: 'İK Mülakatı', value: 'HR' },
        { label: 'Teknik Mülakat', value: 'Teknik' },
        { label: 'Yönetici Mülakatı', value: 'Yonetici' },
        { label: 'Genel Müdür Mülakatı', value: 'GenelMudur' },
        { label: 'Video Mülakat', value: 'Video' }
    ];

    // Mülakat durumları
    const mulakatDurumlari = [
        { label: 'Planlandı', value: 'Planlandi' },
        { label: 'Devam Ediyor', value: 'DevamEdiyor' },
        { label: 'Tamamlandı', value: 'Tamamlandi' },
        { label: 'İptal Edildi', value: 'IptalEdildi' }
    ];

    useEffect(() => {
        loadMulakatlar();
        loadBasvurular();
        loadPersoneller();
    }, []);

    const loadMulakatlar = async () => {
        setLoading(true);
        try {
            const response = await iseAlimService.getMulakatlar();
            if (response.success) {
                setMulakatlar(response.data);
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Mülakatlar yüklenirken hata oluştu.' });
        } finally {
            setLoading(false);
        }
    };

    const loadBasvurular = async () => {
        try {
            const response = await iseAlimService.getBasvurular();
            if (response.success) {
                const basvuruOptions = response.data.map(b => ({
                    label: `${b.adayAd} - ${b.ilanBaslik}`,
                    value: b.id
                }));
                setBasvurular(basvuruOptions);
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Başvurular yüklenirken hata oluştu.' });
        }
    };

    const loadPersoneller = async () => {
        try {
            const response = await personelService.getPersonellerAktif();

            if (response.success) {
                const personelOptions = response.data.map(p => ({
                    label: `${p.ad} ${p.soyad}`,
                    value: p.id
                }));
                setPersoneller(personelOptions);
            } else {
                setPersoneller([]);
            }
        } catch (error) {
            // console.error('Personeller yüklenirken hata:', error);
            setPersoneller([]);
        }
    };

    const openNew = () => {
        setMulakat({
            basvuruId: null,
            tur: '',
            tarih: null,
            sure: 60,
            lokasyon: '',
            mulakatYapanId: null,
            notlar: ''
        });
        setSubmitted(false);
        setMulakatDialog(true);
    };

    const editMulakat = async (mulakat) => {
        try {
            // Tek kayıt detaylarını backend'den al
            const response = await iseAlimService.getMulakat(mulakat.id);
            if (response.success) {
                const mulakatDetay = response.data;
                setMulakat({
                    id: mulakatDetay.id,
                    basvuruId: mulakatDetay.basvuruId,
                    tur: mulakatDetay.tur,
                    tarih: new Date(mulakatDetay.tarih),
                    sure: mulakatDetay.sure || 60,
                    lokasyon: mulakatDetay.lokasyon || '',
                    mulakatYapanId: mulakatDetay.mulakatYapanId,
                    notlar: mulakatDetay.notlar || ''
                });
                setMulakatDialog(true);
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Mülakat detayları yüklenirken hata oluştu.' });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Mülakat detayları yüklenirken hata oluştu.' });
        }
    };

    const hideDialog = () => {
        setSubmitted(false);
        setMulakatDialog(false);
    };

    const hideSonucDialog = () => {
        setSonucDialog(false);
    };

    const saveMulakat = async () => {
        setSubmitted(true);

        if (mulakat.basvuruId && mulakat.tur && mulakat.tarih && mulakat.mulakatYapanId) {
            try {
                let response;
                if (mulakat.id) {
                    response = await iseAlimService.updateMulakat(mulakat.id, mulakat);
                } else {
                    response = await iseAlimService.createMulakat(mulakat);
                }

                if (response.success) {
                    toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Mülakat başarıyla kaydedildi.' });
                    loadMulakatlar();
                    hideDialog();
                } else {
                    toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
                }
            } catch (error) {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Mülakat kaydedilirken hata oluştu.' });
            }
        }
    };

    const confirmDelete = (mulakat) => {
        setSelectedMulakat(mulakat);
        setDeleteDialog(true);
    };

    const deleteMulakat = async () => {
        try {
            const response = await iseAlimService.deleteMulakat(selectedMulakat.id);
            if (response.success) {
                setMulakatlar(mulakatlar.filter(val => val.id !== selectedMulakat.id));
                setDeleteDialog(false);
                setSelectedMulakat(null);
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Mülakat başarıyla silindi.' });
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Mülakat silinirken hata oluştu.' });
        }
    };

    const tamamlaMulakat = (mulakat) => {
        setSelectedMulakat(mulakat);
        setSonucDialog(true);
    };

    const exportCSV = () => {
        dt.current?.exportCSV();
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _mulakat = { ...mulakat };

        // Sure alanı için number tipine çevir
        if (name === 'sure') {
            _mulakat[`${name}`] = val ? parseInt(val, 10) : 60;
        } else {
            _mulakat[`${name}`] = val;
        }

        setMulakat(_mulakat);
    };

    const onCalendarChange = (e, name) => {
        const val = e.value;
        let _mulakat = { ...mulakat };
        _mulakat[`${name}`] = val;
        setMulakat(_mulakat);
    };

    const onDropdownChange = (e, name) => {
        const val = e.value;
        let _mulakat = { ...mulakat };
        _mulakat[`${name}`] = val;
        setMulakat(_mulakat);
    };

    // Template functions
    const tarihBodyTemplate = (rowData) => {
        return rowData.tarih ? new Date(rowData.tarih).toLocaleDateString('tr-TR', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }) : '';
    };

    const turBodyTemplate = (rowData) => {
        return <Badge value={rowData.turText} severity="info" />;
    };

    const durumBodyTemplate = (rowData) => {
        let severity = 'secondary';
        switch (rowData.durum) {
            case 'Planlandi':
                severity = 'warning';
                break;
            case 'DevamEdiyor':
                severity = 'info';
                break;
            case 'Tamamlandi':
                severity = 'success';
                break;
            case 'IptalEdildi':
                severity = 'danger';
                break;
            default:
                severity = 'secondary';
        }
        return <Badge value={rowData.durum} severity={severity} />;
    };

    const puanBodyTemplate = (rowData) => {
        if (rowData.puan && rowData.puan > 0) {
            return <Rating value={rowData.puan / 20} stars={5} readOnly cancel={false} />;
        }
        return '-';
    };

    const notlarBodyTemplate = (rowData) => {
        if (!rowData.notlar) return '-';
        const maxLength = 50;
        if (rowData.notlar.length > maxLength) {
            return (
                <span title={rowData.notlar}>
                    {rowData.notlar.substring(0, maxLength)}...
                </span>
            );
        }
        return rowData.notlar;
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button icon="pi pi-pencil" className="p-button-rounded p-button-success p-button-sm"
                        onClick={() => editMulakat(rowData)} tooltip="Düzenle" />
                {rowData.durum === 'Planlandi' && (
                    <Button icon="pi pi-check" className="p-button-rounded p-button-info p-button-sm"
                            onClick={() => tamamlaMulakat(rowData)} tooltip="Tamamla" />
                )}
                <Button icon="pi pi-trash" className="p-button-rounded p-button-danger p-button-sm"
                        onClick={() => confirmDelete(rowData)} tooltip="Sil" />
            </div>
        );
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex gap-2">
                <Button label="Yeni Mülakat" icon="pi pi-plus" className="p-button-success" onClick={openNew} />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <div className="flex gap-2">
                <Button label="Excel" icon="pi pi-upload" className="p-button-help" onClick={exportCSV} />
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText value={globalFilter} onChange={(e) => setGlobalFilter(e.target.value)} placeholder="Arama..." />
                </span>
            </div>
        );
    };

    const mulakatDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveMulakat} />
        </div>
    );

    const deleteDialogFooter = (
        <div>
            <Button label="Hayır" icon="pi pi-times" className="p-button-text" onClick={() => setDeleteDialog(false)} />
            <Button label="Evet" icon="pi pi-check" className="p-button-text" onClick={deleteMulakat} />
        </div>
    );

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            <div className="card">
                <h3 className="mb-4">Mülakat Takvimi</h3>

                <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate}></Toolbar>

                <DataTable
                    ref={dt}
                    value={mulakatlar}
                    selection={selectedMulakat}
                    onSelectionChange={(e) => setSelectedMulakat(e.value)}
                    dataKey="id"
                    paginator
                    rows={10}
                    rowsPerPageOptions={[5, 10, 25]}
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    currentPageReportTemplate="{first} - {last} / {totalRecords} mülakat"
                    globalFilter={globalFilter}
                    emptyMessage="Mülakat bulunamadı."
                    loading={loading}
                    filters={filters}
                    filterDisplay="menu"
                    responsiveLayout="scroll">

                    <Column field="adayAd" header="Aday" sortable filter filterPlaceholder="Aday ara" style={{ minWidth: '14rem' }} />
                    <Column field="ilanBaslik" header="İlan" sortable filter filterPlaceholder="İlan ara" style={{ minWidth: '12rem' }} />
                    <Column field="turText" header="Tür" body={turBodyTemplate} sortable style={{ minWidth: '10rem' }} />
                    <Column field="tarih" header="Tarih" body={tarihBodyTemplate} sortable style={{ minWidth: '12rem' }} />
                    <Column field="sure" header="Süre (dk)" sortable style={{ minWidth: '8rem' }} />
                    <Column field="lokasyon" header="Lokasyon" sortable style={{ minWidth: '12rem' }} />
                    <Column field="mulakatYapan" header="Mülakat Yapan" sortable style={{ minWidth: '12rem' }} />
                    <Column field="durum" header="Durum" body={durumBodyTemplate} sortable style={{ minWidth: '10rem' }} />
                    <Column field="notlar" header="Notlar" body={notlarBodyTemplate} sortable style={{ minWidth: '14rem' }} />
                    <Column field="puan" header="Puan" body={puanBodyTemplate} style={{ minWidth: '8rem' }} />
                    <Column body={actionBodyTemplate} exportable={false} style={{ minWidth: '12rem' }}></Column>
                </DataTable>
            </div>

            {/* Mülakat Dialog */}
            <Dialog visible={mulakatDialog} style={{ width: '600px' }} header="Mülakat Detayları" modal className="p-fluid" footer={mulakatDialogFooter} onHide={hideDialog}>
                <div className="field">
                    <label htmlFor="basvuruId">Başvuru <span className="text-red-500">*</span></label>
                    <Dropdown id="basvuruId" value={mulakat.basvuruId} onChange={(e) => onDropdownChange(e, 'basvuruId')} options={basvurular}
                              placeholder="Başvuru seçin" className={submitted && !mulakat.basvuruId ? 'p-invalid' : ''} filter />
                    {submitted && !mulakat.basvuruId && <small className="p-error">Başvuru seçimi gereklidir.</small>}
                </div>

                <div className="field">
                    <label htmlFor="tur">Mülakat Türü <span className="text-red-500">*</span></label>
                    <Dropdown id="tur" value={mulakat.tur} onChange={(e) => onDropdownChange(e, 'tur')} options={mulakatTurleri}
                              placeholder="Mülakat türü seçin" className={submitted && !mulakat.tur ? 'p-invalid' : ''} />
                    {submitted && !mulakat.tur && <small className="p-error">Mülakat türü gereklidir.</small>}
                </div>

                <div className="field">
                    <label htmlFor="tarih">Mülakat Tarihi <span className="text-red-500">*</span></label>
                    <Calendar id="tarih" value={mulakat.tarih} onChange={(e) => onCalendarChange(e, 'tarih')}
                              showTime hourFormat="24" dateFormat="dd/mm/yy" placeholder="Tarih ve saat seçin"
                              locale="tr" className={submitted && !mulakat.tarih ? 'p-invalid' : ''} />
                    {submitted && !mulakat.tarih && <small className="p-error">Mülakat tarihi gereklidir.</small>}
                </div>

                <div className="field">
                    <label htmlFor="sure">Süre (Dakika)</label>
                    <InputText id="sure" value={mulakat.sure} onChange={(e) => onInputChange(e, 'sure')} type="number" />
                </div>

                <div className="field">
                    <label htmlFor="mulakatYapanId">Mülakat Yapan <span className="text-red-500">*</span></label>
                    <Dropdown id="mulakatYapanId" value={mulakat.mulakatYapanId} onChange={(e) => onDropdownChange(e, 'mulakatYapanId')} options={personeller}
                              placeholder="Mülakat yapan personel seçin" className={submitted && !mulakat.mulakatYapanId ? 'p-invalid' : ''} filter />
                    {submitted && !mulakat.mulakatYapanId && <small className="p-error">Mülakat yapan seçimi gereklidir.</small>}
                </div>

                <div className="field">
                    <label htmlFor="lokasyon">Lokasyon</label>
                    <InputText id="lokasyon" value={mulakat.lokasyon} onChange={(e) => onInputChange(e, 'lokasyon')} />
                </div>

                <div className="field">
                    <label htmlFor="notlar">Notlar</label>
                    <InputTextarea id="notlar" value={mulakat.notlar} onChange={(e) => onInputChange(e, 'notlar')} rows={3} cols={20} />
                </div>
            </Dialog>

            {/* Delete Dialog */}
            <Dialog visible={deleteDialog} style={{ width: '450px' }} header="Onayla" modal footer={deleteDialogFooter} onHide={() => setDeleteDialog(false)}>
                <div className="flex align-items-center justify-content-center">
                    <i className="pi pi-exclamation-triangle mr-3" style={{ fontSize: '2rem' }} />
                    {selectedMulakat && (
                        <span>
                            <b>{selectedMulakat.adayAd}</b> adayının mülakatını silmek istediğinizden emin misiniz?
                        </span>
                    )}
                </div>
            </Dialog>

            <ConfirmDialog />
        </div>
    );
};

export default MulakatTakvimi;