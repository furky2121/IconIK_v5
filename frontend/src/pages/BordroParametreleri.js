import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { InputSwitch } from 'primereact/inputswitch';
import { Dropdown } from 'primereact/dropdown';
import { Divider } from 'primereact/divider';
import { Panel } from 'primereact/panel';
import bordroParametreService from '../services/bordroParametreService';
import yetkiService from '../services/yetkiService';

const BordroParametreleri = () => {
    const emptyParametre = {
        id: null,
        yil: new Date().getFullYear(),
        donem: 1,
        asgariBrutUcret: 20002.50,
        asgariBrutUcretNet: 17002.12,
        sgkTabanTutari: 20002.50,
        sgkTavanTutari: 147073.50,
        sgkIsciOrani: 14.0,
        sgkIsverenOrani: 20.5,
        issizlikIsciOrani: 1.0,
        issizlikIsverenOrani: 2.0,
        damgaVergisiOrani: 0.759,
        agiOrani: 15.0,
        agiTutari: 2140.20,
        vergiDilimi1Tavan: 110000,
        vergiDilimi1Oran: 15.0,
        vergiDilimi2Tavan: 230000,
        vergiDilimi2Oran: 20.0,
        vergiDilimi3Tavan: 870000,
        vergiDilimi3Oran: 27.0,
        vergiDilimi4Tavan: 3000000,
        vergiDilimi4Oran: 35.0,
        vergiDilimi5Oran: 40.0,
        aktif: true,
        aciklama: ''
    };

    const [parametreler, setParametreler] = useState([]);
    const [parametreDialog, setParametreDialog] = useState(false);
    const [parametre, setParametre] = useState(emptyParametre);
    const [selectedParametreler, setSelectedParametreler] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        donem: false
    });
    const toast = useRef(null);
    const dt = useRef(null);

    const donemOptions = [
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
        loadParametreler();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('bordro-parametreleri', 'read'),
                write: yetkiService.hasScreenPermission('bordro-parametreleri', 'write'),
                delete: yetkiService.hasScreenPermission('bordro-parametreleri', 'delete'),
                update: yetkiService.hasScreenPermission('bordro-parametreleri', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false,
                write: false,
                delete: false,
                update: false
            });
        }
    };

    const loadParametreler = async () => {
        setLoading(true);
        try {
            const response = await bordroParametreService.getAll();
            if (response.success) {
                setParametreler(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Parametreler yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        setParametre(emptyParametre);
        setSubmitted(false);
        setParametreDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setParametreDialog(false);
    };

    const saveParametre = async () => {
        setSubmitted(true);

        if (parametre.yil && parametre.donem && !saving) {
            setSaving(true);
            try {
                const dataToSend = { ...parametre };
                if (!dataToSend.id) {
                    delete dataToSend.id;
                }

                let response;
                if (parametre.id) {
                    response = await bordroParametreService.update(parametre.id, dataToSend);
                } else {
                    response = await bordroParametreService.create(dataToSend);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadParametreler();
                    setParametreDialog(false);
                    setParametre(emptyParametre);
                }
            } catch (error) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: error.message,
                    life: 3000
                });
            } finally {
                setSaving(false);
            }
        }
    };

    const editParametre = (parametre) => {
        setParametre({ ...parametre });
        setParametreDialog(true);
    };

    const confirmDeleteParametre = (parametre) => {
        confirmDialog({
            message: `${parametre.yil}/${parametre.donem} dönemine ait parametreyi silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteParametre(parametre.id)
        });
    };

    const deleteParametre = async (id) => {
        try {
            const response = await bordroParametreService.delete(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadParametreler();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message,
                life: 3000
            });
        }
    };

    const exportCSV = () => {
        dt.current.exportCSV();
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _parametre = { ...parametre };
        _parametre[`${name}`] = val;
        setParametre(_parametre);
    };

    const onInputNumberChange = (e, name) => {
        const val = e.value || 0;
        let _parametre = { ...parametre };
        _parametre[`${name}`] = val;
        setParametre(_parametre);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Parametre"
                        icon="pi pi-plus"
                        className="p-button-success p-mr-2"
                        onClick={openNew}
                    />
                )}
            </React.Fragment>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <Button
                    label="Dışa Aktar"
                    icon="pi pi-upload"
                    className="p-button-help"
                    onClick={exportCSV}
                />
            </React.Fragment>
        );
    };

    const statusBodyTemplate = (rowData) => {
        const badgeStyle = {
            padding: '0.25rem 0.5rem',
            borderRadius: '0.25rem',
            fontSize: '0.875rem',
            fontWeight: '600',
            color: 'white',
            backgroundColor: rowData.aktif ? '#22c55e' : '#ef4444'
        };

        return (
            <span style={badgeStyle}>
                {rowData.aktif ? 'Aktif' : 'Pasif'}
            </span>
        );
    };

    const donemBodyTemplate = (rowData) => {
        const donem = donemOptions.find(d => d.value === rowData.donem);
        return donem ? donem.label : rowData.donem;
    };

    const currencyBodyTemplate = (rowData, field) => {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(rowData[field]);
    };

    const percentBodyTemplate = (rowData, field) => {
        return `%${rowData[field].toFixed(2)}`;
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editParametre(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteParametre(rowData)}
                        tooltip="Sil"
                    />
                )}
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Bordro Parametreleri Yönetimi</h5>
            <span className="p-input-icon-left">
                <i className="pi pi-search" />
                <InputText
                    type="search"
                    onInput={(e) => setGlobalFilter(e.target.value)}
                    placeholder="Arama yapın..."
                />
            </span>
        </div>
    );

    const parametreDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideDialog}
            />
            <Button
                label="Kaydet"
                icon="pi pi-check"
                className="p-button-text"
                onClick={saveParametre}
                disabled={saving}
            />
        </React.Fragment>
    );

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            <Card>
                <Toolbar
                    className="p-mb-4"
                    left={leftToolbarTemplate}
                    right={rightToolbarTemplate}
                ></Toolbar>

                <DataTable
                    ref={dt}
                    value={parametreler}
                    selection={selectedParametreler}
                    onSelectionChange={(e) => setSelectedParametreler(e.value)}
                    dataKey="id"
                    paginator
                    rows={10}
                    rowsPerPageOptions={[5, 10, 25]}
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} kayıt"
                    globalFilter={globalFilter}
                    header={header}
                    responsiveLayout="scroll"
                    loading={loading}
                >
                    <Column
                        field="id"
                        header="ID"
                        sortable
                        style={{ minWidth: '4rem', width: '4rem' }}
                    ></Column>
                    <Column
                        field="yil"
                        header="Yıl"
                        sortable
                        style={{ minWidth: '6rem' }}
                    ></Column>
                    <Column
                        field="donem"
                        header="Dönem"
                        body={donemBodyTemplate}
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="asgariBrutUcret"
                        header="Asgari Ücret (Brüt)"
                        body={(rowData) => currencyBodyTemplate(rowData, 'asgariBrutUcret')}
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="sgkIsciOrani"
                        header="SGK İşçi %"
                        body={(rowData) => percentBodyTemplate(rowData, 'sgkIsciOrani')}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="aktif"
                        header="Durum"
                        body={statusBodyTemplate}
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        body={actionBodyTemplate}
                        header="İşlemler"
                        style={{ minWidth: '8rem' }}
                    ></Column>
                </DataTable>
            </Card>

            <Dialog
                visible={parametreDialog}
                style={{ width: '900px' }}
                header="Bordro Parametreleri Detayları"
                modal
                className="p-fluid"
                footer={parametreDialogFooter}
                onHide={hideDialog}
            >
                <Panel header="Genel Bilgiler" className="p-mb-3">
                    <div className="p-grid p-fluid">
                        <div className="p-col-12 p-md-4">
                            <div className="p-field">
                                <label htmlFor="yil">Yıl *</label>
                                <InputNumber
                                    id="yil"
                                    value={parametre.yil}
                                    onValueChange={(e) => onInputNumberChange(e, 'yil')}
                                    useGrouping={false}
                                    className={submitted && !parametre.yil ? 'p-invalid' : ''}
                                />
                                {submitted && !parametre.yil && (
                                    <small className="p-error">Yıl gereklidir.</small>
                                )}
                            </div>
                        </div>

                        <div className="p-col-12 p-md-4">
                            <div className="p-field">
                                <label htmlFor="donem">Dönem *</label>
                                <Dropdown
                                    id="donem"
                                    value={parametre.donem}
                                    options={donemOptions}
                                    onChange={(e) => onInputChange(e, 'donem')}
                                    placeholder="Dönem seçin"
                                    className={submitted && !parametre.donem ? 'p-invalid' : ''}
                                />
                                {submitted && !parametre.donem && (
                                    <small className="p-error">Dönem gereklidir.</small>
                                )}
                            </div>
                        </div>

                        <div className="p-col-12 p-md-4">
                            <div className="p-field">
                                <label htmlFor="aktif">Durum</label>
                                <div>
                                    <InputSwitch
                                        id="aktif"
                                        checked={parametre.aktif}
                                        onChange={(e) => setParametre({ ...parametre, aktif: e.value })}
                                    />
                                    <span className="p-ml-2">
                                        {parametre.aktif ? 'Aktif' : 'Pasif'}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </Panel>

                <Panel header="Asgari Ücret Bilgileri" className="p-mb-3">
                    <div className="p-grid p-fluid">
                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="asgariBrutUcret">Asgari Brüt Ücret (TL)</label>
                                <InputNumber
                                    id="asgariBrutUcret"
                                    value={parametre.asgariBrutUcret}
                                    onValueChange={(e) => onInputNumberChange(e, 'asgariBrutUcret')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="asgariBrutUcretNet">Asgari Net Ücret (TL)</label>
                                <InputNumber
                                    id="asgariBrutUcretNet"
                                    value={parametre.asgariBrutUcretNet}
                                    onValueChange={(e) => onInputNumberChange(e, 'asgariBrutUcretNet')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                />
                            </div>
                        </div>
                    </div>
                </Panel>

                <Panel header="SGK Parametreleri" className="p-mb-3">
                    <div className="p-grid p-fluid">
                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="sgkTabanTutari">SGK Taban Tutarı (TL)</label>
                                <InputNumber
                                    id="sgkTabanTutari"
                                    value={parametre.sgkTabanTutari}
                                    onValueChange={(e) => onInputNumberChange(e, 'sgkTabanTutari')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="sgkTavanTutari">SGK Tavan Tutarı (TL)</label>
                                <InputNumber
                                    id="sgkTavanTutari"
                                    value={parametre.sgkTavanTutari}
                                    onValueChange={(e) => onInputNumberChange(e, 'sgkTavanTutari')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="sgkIsciOrani">SGK İşçi Oranı (%)</label>
                                <InputNumber
                                    id="sgkIsciOrani"
                                    value={parametre.sgkIsciOrani}
                                    onValueChange={(e) => onInputNumberChange(e, 'sgkIsciOrani')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="sgkIsverenOrani">SGK İşveren Oranı (%)</label>
                                <InputNumber
                                    id="sgkIsverenOrani"
                                    value={parametre.sgkIsverenOrani}
                                    onValueChange={(e) => onInputNumberChange(e, 'sgkIsverenOrani')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>
                    </div>
                </Panel>

                <Panel header="İşsizlik Sigortası" className="p-mb-3">
                    <div className="p-grid p-fluid">
                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="issizlikIsciOrani">İşsizlik İşçi Oranı (%)</label>
                                <InputNumber
                                    id="issizlikIsciOrani"
                                    value={parametre.issizlikIsciOrani}
                                    onValueChange={(e) => onInputNumberChange(e, 'issizlikIsciOrani')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="issizlikIsverenOrani">İşsizlik İşveren Oranı (%)</label>
                                <InputNumber
                                    id="issizlikIsverenOrani"
                                    value={parametre.issizlikIsverenOrani}
                                    onValueChange={(e) => onInputNumberChange(e, 'issizlikIsverenOrani')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>
                    </div>
                </Panel>

                <Panel header="Damga Vergisi ve AGİ" className="p-mb-3">
                    <div className="p-grid p-fluid">
                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="damgaVergisiOrani">Damga Vergisi Oranı (%)</label>
                                <InputNumber
                                    id="damgaVergisiOrani"
                                    value={parametre.damgaVergisiOrani}
                                    onValueChange={(e) => onInputNumberChange(e, 'damgaVergisiOrani')}
                                    mode="decimal"
                                    minFractionDigits={3}
                                    maxFractionDigits={3}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-3">
                            <div className="p-field">
                                <label htmlFor="agiOrani">AGİ Oranı (%)</label>
                                <InputNumber
                                    id="agiOrani"
                                    value={parametre.agiOrani}
                                    onValueChange={(e) => onInputNumberChange(e, 'agiOrani')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-3">
                            <div className="p-field">
                                <label htmlFor="agiTutari">AGİ Tutarı (TL)</label>
                                <InputNumber
                                    id="agiTutari"
                                    value={parametre.agiTutari}
                                    onValueChange={(e) => onInputNumberChange(e, 'agiTutari')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                />
                            </div>
                        </div>
                    </div>
                </Panel>

                <Panel header="Gelir Vergisi Dilimleri" className="p-mb-3">
                    <div className="p-grid p-fluid">
                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi1Tavan">1. Dilim Tavan (TL)</label>
                                <InputNumber
                                    id="vergiDilimi1Tavan"
                                    value={parametre.vergiDilimi1Tavan}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi1Tavan')}
                                    mode="decimal"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi1Oran">1. Dilim Oran (%)</label>
                                <InputNumber
                                    id="vergiDilimi1Oran"
                                    value={parametre.vergiDilimi1Oran}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi1Oran')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi2Tavan">2. Dilim Tavan (TL)</label>
                                <InputNumber
                                    id="vergiDilimi2Tavan"
                                    value={parametre.vergiDilimi2Tavan}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi2Tavan')}
                                    mode="decimal"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi2Oran">2. Dilim Oran (%)</label>
                                <InputNumber
                                    id="vergiDilimi2Oran"
                                    value={parametre.vergiDilimi2Oran}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi2Oran')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi3Tavan">3. Dilim Tavan (TL)</label>
                                <InputNumber
                                    id="vergiDilimi3Tavan"
                                    value={parametre.vergiDilimi3Tavan}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi3Tavan')}
                                    mode="decimal"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi3Oran">3. Dilim Oran (%)</label>
                                <InputNumber
                                    id="vergiDilimi3Oran"
                                    value={parametre.vergiDilimi3Oran}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi3Oran')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi4Tavan">4. Dilim Tavan (TL)</label>
                                <InputNumber
                                    id="vergiDilimi4Tavan"
                                    value={parametre.vergiDilimi4Tavan}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi4Tavan')}
                                    mode="decimal"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi4Oran">4. Dilim Oran (%)</label>
                                <InputNumber
                                    id="vergiDilimi4Oran"
                                    value={parametre.vergiDilimi4Oran}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi4Oran')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>

                        <div className="p-col-12 p-md-6">
                            <div className="p-field">
                                <label htmlFor="vergiDilimi5Oran">5. Dilim Oran (%) - Üst Sınırsız</label>
                                <InputNumber
                                    id="vergiDilimi5Oran"
                                    value={parametre.vergiDilimi5Oran}
                                    onValueChange={(e) => onInputNumberChange(e, 'vergiDilimi5Oran')}
                                    mode="decimal"
                                    minFractionDigits={2}
                                    maxFractionDigits={2}
                                    suffix="%"
                                />
                            </div>
                        </div>
                    </div>
                </Panel>

                <div className="p-field">
                    <label htmlFor="aciklama">Açıklama</label>
                    <InputTextarea
                        id="aciklama"
                        value={parametre.aciklama}
                        onChange={(e) => onInputChange(e, 'aciklama')}
                        rows={3}
                        cols={20}
                    />
                </div>
            </Dialog>
        </div>
    );
};

export default BordroParametreleri;
