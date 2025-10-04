'use client';

import { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { InputNumber } from 'primereact/inputnumber';
import { Calendar } from 'primereact/calendar';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { ConfirmDialog } from 'primereact/confirmdialog';
import iseAlimService from '../services/iseAlimService';
import departmanService from '../services/departmanService';
import pozisyonService from '../services/pozisyonService';

const IsIlanlari = () => {
    const [isIlanlari, setIsIlanlari] = useState([]);
    const [kategoriler, setKategoriler] = useState([]);
    const [departmanlar, setDepartmanlar] = useState([]);
    const [pozisyonlar, setPozisyonlar] = useState([]);
    const [ilanDialog, setIlanDialog] = useState(false);
    const [deleteIlanDialog, setDeleteIlanDialog] = useState(false);
    const [ilan, setIlan] = useState({});
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState('');
    const [filters, setFilters] = useState({
        'baslik': { value: null, matchMode: 'contains' },
        'kategori': { value: null, matchMode: 'contains' },
        'pozisyon': { value: null, matchMode: 'contains' },
        'departman': { value: null, matchMode: 'contains' },
        'durum': { value: null, matchMode: 'equals' },
        'yayinTarihi': { value: null, matchMode: 'dateIs' }
    });
    const [loading, setLoading] = useState(false);
    const toast = useRef(null);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        setLoading(true);
        try {
            const [ilanlarRes, kategorilerRes, departmanlarRes, pozisyonlarRes] = await Promise.all([
                iseAlimService.getIsIlanlari(),
                iseAlimService.getAktifIlanKategorileri(),
                departmanService.getAktifDepartmanlar(),
                pozisyonService.getAktifPozisyonlar()
            ]);

            setIsIlanlari(ilanlarRes.data);
            setKategoriler(kategorilerRes.data);
            setDepartmanlar(departmanlarRes.data);
            setPozisyonlar(pozisyonlarRes.data);
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Veriler yüklenirken hata oluştu' });
        }
        setLoading(false);
    };

    const openNew = () => {
        setIlan({
            baslik: '',
            kategoriId: null,
            pozisyonId: null,
            departmanId: null,
            isTanimi: '',
            gereksinimler: '',
            minMaas: null,
            maxMaas: null,
            calismaSekli: null,
            deneyimYili: 0,
            egitimSeviyesi: null,
            yayinTarihi: null,
            bitisTarihi: null,
            aktif: true
        });
        setSubmitted(false);
        setIlanDialog(true);
    };

    const editIlan = (ilan) => {
        setIlan({
            ...ilan,
            yayinTarihi: ilan.yayinTarihi ? new Date(ilan.yayinTarihi) : null,
            bitisTarihi: ilan.bitisTarihi ? new Date(ilan.bitisTarihi) : null
        });
        setIlanDialog(true);
    };

    const confirmDeleteIlan = (ilan) => {
        setIlan(ilan);
        setDeleteIlanDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setIlanDialog(false);
    };

    const hideDeleteIlanDialog = () => {
        setDeleteIlanDialog(false);
    };

    const saveIlan = async () => {
        setSubmitted(true);

        // Validation
        if (!ilan.baslik?.trim()) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'İlan başlığı gereklidir.' });
            return;
        }
        if (!ilan.kategoriId) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Kategori seçimi gereklidir.' });
            return;
        }
        if (!ilan.pozisyonId) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Pozisyon seçimi gereklidir.' });
            return;
        }
        if (!ilan.departmanId) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Departman seçimi gereklidir.' });
            return;
        }
        if (!ilan.isTanimi?.trim()) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'İş tanımı gereklidir.' });
            return;
        }
        if (!ilan.gereksinimler?.trim()) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Gereksinimler gereklidir.' });
            return;
        }

        let _isIlanlari = [...isIlanlari];
        let _ilan = { ...ilan };

        try {
            if (_ilan.id) {
                const response = await iseAlimService.updateIsIlani(_ilan.id, _ilan);
                if (response.success) {
                    toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'İş ilanı güncellendi' });
                    loadData(); // Refresh data
                }
            } else {
                const response = await iseAlimService.createIsIlani({
                    ..._ilan,
                    olusturanId: 1 // TODO: Get from auth context
                });
                if (response.success) {
                    toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'İş ilanı oluşturuldu' });
                    loadData(); // Refresh data
                }
            }

            setIlanDialog(false);
            setIlan({});
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'İşlem sırasında hata oluştu'
            });
        }
    };

    const deleteIlan = async () => {
        try {
            const response = await iseAlimService.deleteIsIlani(ilan.id);
            if (response.success) {
                let _isIlanlari = isIlanlari.filter(i => i.id !== ilan.id);
                setIsIlanlari(_isIlanlari);
                setDeleteIlanDialog(false);
                setIlan({});
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'İş ilanı silindi' });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'Silme işlemi sırasında hata oluştu'
            });
        }
    };

    const yayinlaIlan = async (ilanData) => {
        try {
            const response = await iseAlimService.yayinlaIsIlani(ilanData.id);
            if (response.success) {
                loadData();
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'İş ilanı yayınlandı' });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'Yayınlama sırasında hata oluştu'
            });
        }
    };

    const kapatIlan = async (ilanData) => {
        try {
            const response = await iseAlimService.kapatIsIlani(ilanData.id);
            if (response.success) {
                loadData();
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'İş ilanı kapatıldı' });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'Kapatma sırasında hata oluştu'
            });
        }
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _ilan = { ...ilan };
        _ilan[`${name}`] = val;
        setIlan(_ilan);
    };

    const onInputNumberChange = (e, name) => {
        const val = e.value || null;
        let _ilan = { ...ilan };
        _ilan[`${name}`] = val;
        setIlan(_ilan);
    };

    const onDropdownChange = (e, name) => {
        const val = e.value;
        let _ilan = { ...ilan };
        _ilan[`${name}`] = val;
        setIlan(_ilan);
    };

    const onDateChange = (e, name) => {
        const val = e.value;
        let _ilan = { ...ilan };
        _ilan[`${name}`] = val;
        setIlan(_ilan);
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                <Button label="Yeni" icon="pi pi-plus" className="p-button-success" onClick={openNew} />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <div className="flex align-items-center gap-2">
                <span className="p-input-icon-left" style={{ width: '300px' }}>
                    <i className="pi pi-search" />
                    <InputText
                        type="search"
                        onInput={(e) => setGlobalFilter(e.target.value)}
                        placeholder="Genel arama..."
                        className="w-full"
                        size="small"
                    />
                </span>
            </div>
        );
    };

    const durumBodyTemplate = (rowData) => {
        return <Tag value={rowData.durum} severity={iseAlimService.getIlanDurumSeverity(rowData.durum)} />;
    };

    const maasBodyTemplate = (rowData) => {
        if (rowData.minMaas && rowData.maxMaas) {
            return `${iseAlimService.formatCurrency(rowData.minMaas)} - ${iseAlimService.formatCurrency(rowData.maxMaas)}`;
        } else if (rowData.minMaas) {
            return `${iseAlimService.formatCurrency(rowData.minMaas)}+`;
        } else if (rowData.maxMaas) {
            return `${iseAlimService.formatCurrency(rowData.maxMaas)}'e kadar`;
        }
        return 'Belirtilmemiş';
    };

    const tarihBodyTemplate = (rowData) => {
        return iseAlimService.formatDateShort(rowData.yayinTarihi);
    };

    const statusFilterTemplate = (options) => {
        return (
            <Dropdown
                value={options.value}
                options={[
                    { label: 'Taslak', value: 'Taslak' },
                    { label: 'Aktif', value: 'Aktif' },
                    { label: 'Kapal\u0131', value: 'Kapali' },
                    { label: 'S\u00fcresi Dolmu\u015f', value: 'SuresiDolmus' }
                ]}
                onChange={(e) => options.filterApplyCallback(e.value)}
                optionLabel="label"
                placeholder="Durum"
                className="p-column-filter"
                showClear
                style={{
                    width: '85%',
                    fontSize: '0.8rem'
                }}
                inputStyle={{
                    height: '2rem',
                    fontSize: '0.8rem'
                }}
                panelStyle={{
                    fontSize: '0.8rem'
                }}
            />
        );
    };

    const dateFilterTemplate = (options) => {
        return (
            <Calendar
                value={options.value}
                onChange={(e) => options.filterApplyCallback(e.value)}
                dateFormat="dd/mm/yy"
                placeholder="Tarih"
                mask="99/99/9999"
                showIcon
                firstDayOfWeek={1}
                className="p-column-filter"
                style={{
                    width: '85%',
                    fontSize: '0.8rem'
                }}
                inputStyle={{
                    height: '2rem',
                    fontSize: '0.8rem',
                    padding: '0.3rem 0.5rem'
                }}
                panelStyle={{
                    fontSize: '0.8rem'
                }}
            />
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-pencil"
                    className="p-button-rounded p-button-success p-button-sm"
                    onClick={() => editIlan(rowData)}
                />
                {rowData.durum === 'Taslak' && (
                    <Button
                        icon="pi pi-send"
                        className="p-button-rounded p-button-info p-button-sm"
                        onClick={() => yayinlaIlan(rowData)}
                        tooltip="Yayınla"
                    />
                )}
                {rowData.durum === 'Aktif' && (
                    <Button
                        icon="pi pi-times-circle"
                        className="p-button-rounded p-button-warning p-button-sm"
                        onClick={() => kapatIlan(rowData)}
                        tooltip="Kapat"
                    />
                )}
                <Button
                    icon="pi pi-trash"
                    className="p-button-rounded p-button-danger p-button-sm"
                    onClick={() => confirmDeleteIlan(rowData)}
                />
            </div>
        );
    };

    const ilanDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveIlan} />
        </div>
    );

    const deleteIlanDialogFooter = (
        <div>
            <Button label="Hayır" icon="pi pi-times" className="p-button-text" onClick={hideDeleteIlanDialog} />
            <Button label="Evet" icon="pi pi-check" className="p-button-text" onClick={deleteIlan} />
        </div>
    );

    return (
        <div className="card">
            <Toast ref={toast} />

            <div className="flex justify-content-between align-items-center mb-4">
                <h2>İş İlanları</h2>
            </div>

            <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate} />

            <DataTable
                value={isIlanlari}
                paginator
                rows={10}
                rowsPerPageOptions={[5, 10, 25]}
                dataKey="id"
                loading={loading}
                globalFilter={globalFilter}
                filters={filters}
                onFilter={(e) => setFilters(e.filters)}
                filterDisplay="row"
                emptyMessage="İş ilanı bulunamadı."
                sortField="id"
                sortOrder={-1}
                size="small"
                rowClassName={() => 'p-datatable-row-sm'}
                className="p-datatable-gridlines"
                showGridlines
            >
                <Column
                    field="baslik"
                    header={(
                        <div className="flex align-items-center gap-1">
                            <span>İlan Başlığı</span>
                            <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                        </div>
                    )}
                    sortable
                    filter
                    filterPlaceholder="Ara..."
                    filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                    filterStyle={{
                        fontSize: '0.8rem',
                        padding: '0.4rem 0.5rem',
                        height: '2rem',
                        width: '85%',
                        lineHeight: '1.2',
                        minHeight: '2rem'
                    }}
                />
                <Column
                    field="kategori"
                    header={(
                        <div className="flex align-items-center gap-1">
                            <span>Kategori</span>
                            <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                        </div>
                    )}
                    sortable
                    filter
                    filterPlaceholder="Ara..."
                    filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                    filterStyle={{
                        fontSize: '0.8rem',
                        padding: '0.4rem 0.5rem',
                        height: '2rem',
                        width: '85%',
                        lineHeight: '1.2',
                        minHeight: '2rem'
                    }}
                />
                <Column
                    field="pozisyon"
                    header={(
                        <div className="flex align-items-center gap-1">
                            <span>Pozisyon</span>
                            <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                        </div>
                    )}
                    sortable
                    filter
                    filterPlaceholder="Ara..."
                    filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                    filterStyle={{
                        fontSize: '0.8rem',
                        padding: '0.4rem 0.5rem',
                        height: '2rem',
                        width: '85%',
                        lineHeight: '1.2',
                        minHeight: '2rem'
                    }}
                />
                <Column
                    field="departman"
                    header={(
                        <div className="flex align-items-center gap-1">
                            <span>Departman</span>
                            <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                        </div>
                    )}
                    sortable
                    filter
                    filterPlaceholder="Ara..."
                    filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                    filterStyle={{
                        fontSize: '0.8rem',
                        padding: '0.4rem 0.5rem',
                        height: '2rem',
                        width: '85%',
                        lineHeight: '1.2',
                        minHeight: '2rem'
                    }}
                />
                <Column field="deneyimYili" header="Deneyim" sortable body={(data) => iseAlimService.formatDeneyim(data.deneyimYili)} />
                <Column
                    field="durum"
                    header={(
                        <div className="flex align-items-center gap-1">
                            <span>Durum</span>
                            <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                        </div>
                    )}
                    sortable
                    body={durumBodyTemplate}
                    filter
                    filterElement={statusFilterTemplate}
                    filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                />
                <Column header="Maaş" body={maasBodyTemplate} />
                <Column
                    field="yayinTarihi"
                    header={(
                        <div className="flex align-items-center gap-1">
                            <span>Yayın Tarihi</span>
                            <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                        </div>
                    )}
                    sortable
                    body={tarihBodyTemplate}
                    filter
                    filterElement={dateFilterTemplate}
                    filterField="yayinTarihi"
                    dataType="date"
                    filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                />
                <Column field="basvuruSayisi" header="Başvuru" sortable />
                <Column
                    body={actionBodyTemplate}
                    exportable={false}
                    style={{ minWidth: '12rem' }}
                    frozen
                    alignFrozen="right"
                    header="İşlemler"
                />
            </DataTable>

            <Dialog
                visible={ilanDialog}
                style={{ width: '800px' }}
                header="İş İlanı Detayları"
                modal
                className="p-fluid"
                footer={ilanDialogFooter}
                onHide={hideDialog}
            >
                <div className="field">
                    <label htmlFor="baslik">Başlık *</label>
                    <InputText
                        id="baslik"
                        value={ilan.baslik || ''}
                        onChange={(e) => onInputChange(e, 'baslik')}
                        required
                        className={submitted && !ilan.baslik ? 'p-invalid' : ''}
                    />
                    {submitted && !ilan.baslik && <small className="p-error">Başlık gereklidir.</small>}
                </div>

                <div className="formgrid grid">
                    <div className="field col">
                        <label htmlFor="kategoriId">Kategori *</label>
                        <Dropdown
                            id="kategoriId"
                            value={ilan.kategoriId}
                            options={kategoriler}
                            onChange={(e) => onDropdownChange(e, 'kategoriId')}
                            optionLabel="ad"
                            optionValue="id"
                            placeholder="Kategori seçin"
                            className={submitted && !ilan.kategoriId ? 'p-invalid' : ''}
                        />
                        {submitted && !ilan.kategoriId && <small className="p-error">Kategori seçimi gereklidir.</small>}
                    </div>
                    <div className="field col">
                        <label htmlFor="departmanId">Departman *</label>
                        <Dropdown
                            id="departmanId"
                            value={ilan.departmanId}
                            options={departmanlar}
                            onChange={(e) => onDropdownChange(e, 'departmanId')}
                            optionLabel="ad"
                            optionValue="id"
                            placeholder="Departman seçin"
                            className={submitted && !ilan.departmanId ? 'p-invalid' : ''}
                        />
                        {submitted && !ilan.departmanId && <small className="p-error">Departman seçimi gereklidir.</small>}
                    </div>
                    <div className="field col">
                        <label htmlFor="pozisyonId">Pozisyon *</label>
                        <Dropdown
                            id="pozisyonId"
                            value={ilan.pozisyonId}
                            options={pozisyonlar}
                            onChange={(e) => onDropdownChange(e, 'pozisyonId')}
                            optionLabel="ad"
                            optionValue="id"
                            placeholder="Pozisyon seçin"
                            className={submitted && !ilan.pozisyonId ? 'p-invalid' : ''}
                        />
                        {submitted && !ilan.pozisyonId && <small className="p-error">Pozisyon seçimi gereklidir.</small>}
                    </div>
                </div>

                <div className="field">
                    <label htmlFor="isTanimi">İş Tanımı *</label>
                    <InputTextarea
                        id="isTanimi"
                        value={ilan.isTanimi || ''}
                        onChange={(e) => onInputChange(e, 'isTanimi')}
                        required
                        rows={4}
                        className={submitted && !ilan.isTanimi ? 'p-invalid' : ''}
                    />
                    {submitted && !ilan.isTanimi && <small className="p-error">İş tanımı gereklidir.</small>}
                </div>

                <div className="field">
                    <label htmlFor="gereksinimler">Gereksinimler *</label>
                    <InputTextarea
                        id="gereksinimler"
                        value={ilan.gereksinimler || ''}
                        onChange={(e) => onInputChange(e, 'gereksinimler')}
                        required
                        rows={4}
                        className={submitted && !ilan.gereksinimler ? 'p-invalid' : ''}
                    />
                    {submitted && !ilan.gereksinimler && <small className="p-error">Gereksinimler gereklidir.</small>}
                </div>

                <div className="formgrid grid">
                    <div className="field col">
                        <label htmlFor="minMaas">Min. Maaş</label>
                        <InputNumber
                            id="minMaas"
                            value={ilan.minMaas}
                            onValueChange={(e) => onInputNumberChange(e, 'minMaas')}
                            mode="currency"
                            currency="TRY"
                            locale="tr"
                        />
                    </div>
                    <div className="field col">
                        <label htmlFor="maxMaas">Max. Maaş</label>
                        <InputNumber
                            id="maxMaas"
                            value={ilan.maxMaas}
                            onValueChange={(e) => onInputNumberChange(e, 'maxMaas')}
                            mode="currency"
                            currency="TRY"
                            locale="tr"
                        />
                    </div>
                </div>

                <div className="formgrid grid">
                    <div className="field col">
                        <label htmlFor="calismaSekli">Çalışma Şekli</label>
                        <Dropdown
                            id="calismaSekli"
                            value={ilan.calismaSekli}
                            options={iseAlimService.getCalismaSekilleri()}
                            onChange={(e) => onDropdownChange(e, 'calismaSekli')}
                            optionLabel="label"
                            optionValue="value"
                            placeholder="Çalışma şekli seçin"
                        />
                    </div>
                    <div className="field col">
                        <label htmlFor="deneyimYili">Deneyim (Yıl)</label>
                        <InputNumber
                            id="deneyimYili"
                            value={ilan.deneyimYili}
                            onValueChange={(e) => onInputNumberChange(e, 'deneyimYili')}
                            min={0}
                            max={30}
                        />
                    </div>
                    <div className="field col">
                        <label htmlFor="egitimSeviyesi">Eğitim Seviyesi</label>
                        <Dropdown
                            id="egitimSeviyesi"
                            value={ilan.egitimSeviyesi}
                            options={iseAlimService.getEgitimSeviyeleri()}
                            onChange={(e) => onDropdownChange(e, 'egitimSeviyesi')}
                            optionLabel="label"
                            optionValue="value"
                            placeholder="Eğitim seviyesi seçin"
                        />
                    </div>
                </div>

                <div className="formgrid grid">
                    <div className="field col">
                        <label htmlFor="yayinTarihi">Yayın Tarihi</label>
                        <Calendar
                            id="yayinTarihi"
                            value={ilan.yayinTarihi}
                            onChange={(e) => onDateChange(e, 'yayinTarihi')}
                            showIcon
                            dateFormat="dd/mm/yy"
                            firstDayOfWeek={1}
                        />
                    </div>
                    <div className="field col">
                        <label htmlFor="bitisTarihi">Bitiş Tarihi</label>
                        <Calendar
                            id="bitisTarihi"
                            value={ilan.bitisTarihi}
                            onChange={(e) => onDateChange(e, 'bitisTarihi')}
                            showIcon
                            dateFormat="dd/mm/yy"
                            firstDayOfWeek={1}
                        />
                    </div>
                </div>
            </Dialog>

            <ConfirmDialog
                visible={deleteIlanDialog}
                onHide={hideDeleteIlanDialog}
                message="Bu iş ilanını silmek istediğinizden emin misiniz?"
                header="Onay"
                icon="pi pi-exclamation-triangle"
                footer={deleteIlanDialogFooter}
            />
        </div>
    );
};

export default IsIlanlari;