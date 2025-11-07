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
import { Dropdown } from 'primereact/dropdown';
import { Calendar } from 'primereact/calendar';
import { Tag } from 'primereact/tag';
import puantajService from '../services/puantajService';
import personelService from '../services/personelService';
import yetkiService from '../services/yetkiService';

const PuantajGirisi = () => {
    const emptyPuantaj = {
        id: null,
        personelId: null,
        yil: new Date().getFullYear(),
        ay: new Date().getMonth() + 1,
        calismaGunu: 0,
        mesaiSaati: 0,
        fazlaMesai: 0,
        haftalikTatil: 0,
        yillikIzin: 0,
        hastalikIzni: 0,
        mazeretIzni: 0,
        onayDurumu: 'Beklemede',
        aciklama: ''
    };

    const [puantajlar, setPuantajlar] = useState([]);
    const [personeller, setPersoneller] = useState([]);
    const [puantajDialog, setPuantajDialog] = useState(false);
    const [puantaj, setPuantaj] = useState(emptyPuantaj);
    const [selectedPuantajlar, setSelectedPuantajlar] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });
    const toast = useRef(null);
    const dt = useRef(null);

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
        loadPuantajlar();
        loadPersoneller();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('puantaj-girisi', 'read'),
                write: yetkiService.hasScreenPermission('puantaj-girisi', 'write'),
                delete: yetkiService.hasScreenPermission('puantaj-girisi', 'delete'),
                update: yetkiService.hasScreenPermission('puantaj-girisi', 'update')
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

    const loadPuantajlar = async () => {
        setLoading(true);
        try {
            const response = await puantajService.getAll();
            if (response.success) {
                setPuantajlar(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Puantajlar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const loadPersoneller = async () => {
        try {
            const response = await personelService.getAktifPersoneller();
            if (response.success) {
                setPersoneller(response.data.map(p => ({
                    label: `${p.ad} ${p.soyad}`,
                    value: p.id
                })));
            }
        } catch (error) {
            // console.error('Personeller yüklenirken hata:', error);
        }
    };

    const openNew = () => {
        setPuantaj(emptyPuantaj);
        setSubmitted(false);
        setPuantajDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setPuantajDialog(false);
    };

    const savePuantaj = async () => {
        setSubmitted(true);

        if (puantaj.personelId && puantaj.yil && puantaj.ay && !saving) {
            setSaving(true);
            try {
                const dataToSend = { ...puantaj };
                if (!dataToSend.id) {
                    delete dataToSend.id;
                }

                let response;
                if (puantaj.id) {
                    response = await puantajService.update(puantaj.id, dataToSend);
                } else {
                    response = await puantajService.create(dataToSend);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadPuantajlar();
                    setPuantajDialog(false);
                    setPuantaj(emptyPuantaj);
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

    const editPuantaj = (puantaj) => {
        setPuantaj({ ...puantaj });
        setPuantajDialog(true);
    };

    const confirmDeletePuantaj = (puantaj) => {
        confirmDialog({
            message: `${puantaj.personelAd} personeline ait ${puantaj.yil}/${puantaj.ay} puantajını silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deletePuantaj(puantaj.id)
        });
    };

    const deletePuantaj = async (id) => {
        try {
            const response = await puantajService.delete(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadPuantajlar();
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
        let _puantaj = { ...puantaj };
        _puantaj[`${name}`] = val;
        setPuantaj(_puantaj);
    };

    const onInputNumberChange = (e, name) => {
        const val = e.value || 0;
        let _puantaj = { ...puantaj };
        _puantaj[`${name}`] = val;
        setPuantaj(_puantaj);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Puantaj"
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

    const onayDurumuBodyTemplate = (rowData) => {
        const getSeverity = (status) => {
            switch (status) {
                case 'Onaylandi':
                case 'Onaylandı':
                    return 'success';
                case 'Reddedildi':
                    return 'danger';
                case 'Beklemede':
                    return 'warning';
                default:
                    return 'info';
            }
        };

        return <Tag value={rowData.onayDurumu} severity={getSeverity(rowData.onayDurumu)} />;
    };

    const ayBodyTemplate = (rowData) => {
        const ay = ayOptions.find(a => a.value === rowData.ay);
        return ay ? ay.label : rowData.ay;
    };

    const actionBodyTemplate = (rowData) => {
        const canEdit = rowData.onayDurumu === 'Beklemede' || rowData.onayDurumu === 'Reddedildi';

        return (
            <React.Fragment>
                {permissions.update && canEdit && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editPuantaj(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && canEdit && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeletePuantaj(rowData)}
                        tooltip="Sil"
                    />
                )}
                {!canEdit && (
                    <Tag value="Onaylandı" severity="success" />
                )}
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Puantaj Girişi</h5>
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

    const puantajDialogFooter = (
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
                onClick={savePuantaj}
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
                    value={puantajlar}
                    selection={selectedPuantajlar}
                    onSelectionChange={(e) => setSelectedPuantajlar(e.value)}
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
                        field="personelAd"
                        header="Personel"
                        sortable
                        style={{ minWidth: '14rem' }}
                    ></Column>
                    <Column
                        field="yil"
                        header="Yıl"
                        sortable
                        style={{ minWidth: '6rem' }}
                    ></Column>
                    <Column
                        field="ay"
                        header="Ay"
                        body={ayBodyTemplate}
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="calismaGunu"
                        header="Çalışma Günü"
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="mesaiSaati"
                        header="Mesai Saati"
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="fazlaMesai"
                        header="Fazla Mesai"
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="onayDurumu"
                        header="Durum"
                        body={onayDurumuBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        body={actionBodyTemplate}
                        header="İşlemler"
                        style={{ minWidth: '10rem' }}
                    ></Column>
                </DataTable>
            </Card>

            <Dialog
                visible={puantajDialog}
                style={{ width: '700px' }}
                header="Puantaj Detayları"
                modal
                className="p-fluid"
                footer={puantajDialogFooter}
                onHide={hideDialog}
            >
                <div className="p-grid p-fluid">
                    <div className="p-col-12 p-md-6">
                        <div className="p-field">
                            <label htmlFor="personelId">Personel *</label>
                            <Dropdown
                                id="personelId"
                                value={puantaj.personelId}
                                options={personeller}
                                onChange={(e) => onInputChange(e, 'personelId')}
                                placeholder="Personel seçin"
                                filter
                                className={submitted && !puantaj.personelId ? 'p-invalid' : ''}
                            />
                            {submitted && !puantaj.personelId && (
                                <small className="p-error">Personel gereklidir.</small>
                            )}
                        </div>
                    </div>

                    <div className="p-col-12 p-md-3">
                        <div className="p-field">
                            <label htmlFor="yil">Yıl *</label>
                            <InputNumber
                                id="yil"
                                value={puantaj.yil}
                                onValueChange={(e) => onInputNumberChange(e, 'yil')}
                                useGrouping={false}
                                className={submitted && !puantaj.yil ? 'p-invalid' : ''}
                            />
                        </div>
                    </div>

                    <div className="p-col-12 p-md-3">
                        <div className="p-field">
                            <label htmlFor="ay">Ay *</label>
                            <Dropdown
                                id="ay"
                                value={puantaj.ay}
                                options={ayOptions}
                                onChange={(e) => onInputChange(e, 'ay')}
                                placeholder="Ay seçin"
                                className={submitted && !puantaj.ay ? 'p-invalid' : ''}
                            />
                        </div>
                    </div>

                    <div className="p-col-12">
                        <h6>Çalışma Bilgileri</h6>
                    </div>

                    <div className="p-col-12 p-md-4">
                        <div className="p-field">
                            <label htmlFor="calismaGunu">Çalışma Günü</label>
                            <InputNumber
                                id="calismaGunu"
                                value={puantaj.calismaGunu}
                                onValueChange={(e) => onInputNumberChange(e, 'calismaGunu')}
                                min={0}
                                max={31}
                            />
                        </div>
                    </div>

                    <div className="p-col-12 p-md-4">
                        <div className="p-field">
                            <label htmlFor="mesaiSaati">Mesai Saati</label>
                            <InputNumber
                                id="mesaiSaati"
                                value={puantaj.mesaiSaati}
                                onValueChange={(e) => onInputNumberChange(e, 'mesaiSaati')}
                                min={0}
                                mode="decimal"
                                minFractionDigits={1}
                                maxFractionDigits={1}
                            />
                        </div>
                    </div>

                    <div className="p-col-12 p-md-4">
                        <div className="p-field">
                            <label htmlFor="fazlaMesai">Fazla Mesai (Saat)</label>
                            <InputNumber
                                id="fazlaMesai"
                                value={puantaj.fazlaMesai}
                                onValueChange={(e) => onInputNumberChange(e, 'fazlaMesai')}
                                min={0}
                                mode="decimal"
                                minFractionDigits={1}
                                maxFractionDigits={1}
                            />
                        </div>
                    </div>

                    <div className="p-col-12">
                        <h6>İzin Bilgileri</h6>
                    </div>

                    <div className="p-col-12 p-md-3">
                        <div className="p-field">
                            <label htmlFor="haftalikTatil">Haftalık Tatil</label>
                            <InputNumber
                                id="haftalikTatil"
                                value={puantaj.haftalikTatil}
                                onValueChange={(e) => onInputNumberChange(e, 'haftalikTatil')}
                                min={0}
                                max={10}
                            />
                        </div>
                    </div>

                    <div className="p-col-12 p-md-3">
                        <div className="p-field">
                            <label htmlFor="yillikIzin">Yıllık İzin</label>
                            <InputNumber
                                id="yillikIzin"
                                value={puantaj.yillikIzin}
                                onValueChange={(e) => onInputNumberChange(e, 'yillikIzin')}
                                min={0}
                            />
                        </div>
                    </div>

                    <div className="p-col-12 p-md-3">
                        <div className="p-field">
                            <label htmlFor="hastalikIzni">Hastalık İzni</label>
                            <InputNumber
                                id="hastalikIzni"
                                value={puantaj.hastalikIzni}
                                onValueChange={(e) => onInputNumberChange(e, 'hastalikIzni')}
                                min={0}
                            />
                        </div>
                    </div>

                    <div className="p-col-12 p-md-3">
                        <div className="p-field">
                            <label htmlFor="mazeretIzni">Mazeret İzni</label>
                            <InputNumber
                                id="mazeretIzni"
                                value={puantaj.mazeretIzni}
                                onValueChange={(e) => onInputNumberChange(e, 'mazeretIzni')}
                                min={0}
                            />
                        </div>
                    </div>

                    <div className="p-col-12">
                        <div className="p-field">
                            <label htmlFor="aciklama">Açıklama</label>
                            <InputTextarea
                                id="aciklama"
                                value={puantaj.aciklama}
                                onChange={(e) => onInputChange(e, 'aciklama')}
                                rows={3}
                                cols={20}
                            />
                        </div>
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default PuantajGirisi;
