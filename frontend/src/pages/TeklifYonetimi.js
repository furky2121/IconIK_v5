import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { InputTextarea } from 'primereact/inputtextarea';
import { Calendar } from 'primereact/calendar';
import { Dropdown } from 'primereact/dropdown';
import { Badge } from 'primereact/badge';
import { Toolbar } from 'primereact/toolbar';
import { Toast } from 'primereact/toast';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { Tag } from 'primereact/tag';
import { Card } from 'primereact/card';
import { Divider } from 'primereact/divider';
import teklifService from '../services/teklifService';
import basvuruService from '../services/basvuruService';

const TeklifYonetimi = () => {
    const [teklifler, setTeklifler] = useState([]);
    const [basvurular, setBasvurular] = useState([]);
    const [loading, setLoading] = useState(false);
    const [teklifDialog, setTeklifDialog] = useState(false);
    const [yanitDialog, setYanitDialog] = useState(false);
    const [detayDialog, setDetayDialog] = useState(false);
    const [istatistikDialog, setIstatistikDialog] = useState(false);
    const [teklif, setTeklif] = useState({});
    const [selectedTeklif, setSelectedTeklif] = useState(null);
    const [istatistikler, setIstatistikler] = useState({});
    const [globalFilter, setGlobalFilter] = useState('');
    const [submitted, setSubmitted] = useState(false);
    const toast = useRef(null);

    const durumOptions = [
        { label: 'Beklemede', value: 'Beklemede' },
        { label: 'Gönderildi', value: 'Gönderildi' },
        { label: 'Kabul Edildi', value: 'Kabul Edildi' },
        { label: 'Reddedildi', value: 'Reddedildi' }
    ];

    const yanitOptions = [
        { label: 'Kabul Etti', value: 'KabulEtti' },
        { label: 'Reddetti', value: 'Reddetti' }
    ];

    useEffect(() => {
        loadTeklifler();
        loadBasvurular();
    }, []);

    const loadTeklifler = async () => {
        setLoading(true);
        try {
            const response = await teklifService.getAll();
            if (response.success) {
                setTeklifler(response.data);
            }
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Teklif mektupları yüklenirken hata oluştu' });
        } finally {
            setLoading(false);
        }
    };

    const loadBasvurular = async () => {
        try {
            const response = await basvuruService.getAll();
            if (response.success) {
            // console.log('Başvuru API Response:', response.data);
            // console.log('İlk başvuru örneği:', response.data[0]);

                // Sadece mülakat tamamlanmış başvuruları filtrele ve dropdown için format düzenle
                const uygunBasvurular = response.data
                    .filter(b => b.durum === 'MulakatTamamlandi' || b.durum === 4 || b.durum === '4')
                    .map(b => {
            // console.log('Başvuru objesi:', b);
            // console.log('Aday adı alanları:', { adayAdi: b.adayAdi, AdayAd: b.AdayAd, field4: b[4] });
            // console.log('İlan başlığı alanları:', { ilanBasligi: b.ilanBasligi, IlanBaslik: b.IlanBaslik, field1: b[1] });

                        return {
                            ...b,
                            displayText: `${b.adayAdi || b.AdayAd || b[4] || 'Bilinmeyen Aday'} - ${b.ilanBasligi || b.IlanBaslik || b[1] || 'Bilinmeyen İlan'}`
                        };
                    });
                setBasvurular(uygunBasvurular);
            }
        } catch (error) {
            // console.error('Başvurular yüklenirken hata:', error);
        }
    };

    const loadIstatistikler = async () => {
        try {
            const response = await teklifService.getIstatistikler();
            if (response.success) {
                setIstatistikler(response.data);
                setIstatistikDialog(true);
            }
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'İstatistikler yüklenirken hata oluştu' });
        }
    };

    const openNew = () => {
        setTeklif({
            basvuruId: null,
            pozisyon: '',
            maas: 0,
            ekOdemeler: '',
            izinHakki: 14,
            iseBaslamaTarihi: null,
            gecerlilikTarihi: null
        });
        setSubmitted(false);
        setTeklifDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setTeklifDialog(false);
        setYanitDialog(false);
        setDetayDialog(false);
        setIstatistikDialog(false);
    };

    const saveTeklif = async () => {
        setSubmitted(true);

        if (teklif.basvuruId && teklif.pozisyon?.trim() && teklif.maas > 0) {
            try {
                let response;
                if (teklif.id) {
                    response = await teklifService.update(teklif.id, teklif);
                } else {
                    response = await teklifService.create(teklif);
                }

                if (response.success) {
                    toast.current.show({ severity: 'success', summary: 'Başarılı', detail: response.message });
                    setTeklifDialog(false);
                    setTeklif({});
                    await loadTeklifler();
                } else {
                    toast.current.show({ severity: 'error', summary: 'Hata', detail: response.message });
                }
            } catch (error) {
                toast.current.show({ severity: 'error', summary: 'Hata', detail: 'İşlem sırasında hata oluştu' });
            }
        }
    };

    const editTeklif = (teklif) => {
        setTeklif({
            ...teklif,
            iseBaslamaTarihi: teklif.iseBaslamaTarihi ? new Date(teklif.iseBaslamaTarihi) : null,
            gecerlilikTarihi: teklif.gecerlilikTarihi ? new Date(teklif.gecerlilikTarihi) : null
        });
        setTeklifDialog(true);
    };

    const confirmDeleteTeklif = (teklif) => {
        confirmDialog({
            message: 'Bu teklif mektubunu silmek istediğinizden emin misiniz?',
            header: 'Onay',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteTeklif(teklif)
        });
    };

    const deleteTeklif = async (teklif) => {
        try {
            const response = await teklifService.delete(teklif.id);
            if (response.success) {
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: response.message });
                await loadTeklifler();
            } else {
                toast.current.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Silme işlemi sırasında hata oluştu' });
        }
    };

    const gonderTeklif = async (teklif) => {
        try {
            const response = await teklifService.gonderTeklif(teklif.id);
            if (response.success) {
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: response.message });
                await loadTeklifler();
            } else {
                toast.current.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Gönderme işlemi sırasında hata oluştu' });
        }
    };

    const showYanitDialog = (teklif) => {
        setSelectedTeklif(teklif);
        setYanitDialog(true);
    };

    const saveAdayYaniti = async () => {
        if (selectedTeklif && selectedTeklif.adayYaniti) {
            try {
                const yanitData = {
                    adayYaniti: selectedTeklif.adayYaniti,
                    redNedeni: selectedTeklif.redNedeni || null
                };

                const response = await teklifService.adayYanitiGuncelle(selectedTeklif.id, yanitData);
                if (response.success) {
                    toast.current.show({ severity: 'success', summary: 'Başarılı', detail: response.message });
                    setYanitDialog(false);
                    setSelectedTeklif(null);
                    await loadTeklifler();
                } else {
                    toast.current.show({ severity: 'error', summary: 'Hata', detail: response.message });
                }
            } catch (error) {
                toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Yanıt kaydı sırasında hata oluştu' });
            }
        }
    };

    const showDetayDialog = (teklif) => {
        setSelectedTeklif(teklif);
        setDetayDialog(true);
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                <Button label="Yeni Teklif" icon="pi pi-plus" className="p-button-success" onClick={openNew} />
                <Button label="İstatistikler" icon="pi pi-chart-bar" className="p-button-info" onClick={loadIstatistikler} />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <div className="flex align-items-center gap-2">
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText
                        type="search"
                        placeholder="Ara..."
                        value={globalFilter}
                        onChange={(e) => setGlobalFilter(e.target.value)}
                    />
                </span>
            </div>
        );
    };

    const durumBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.durum}
                severity={teklifService.getDurumSeviyesi(rowData.durum)}
            />
        );
    };

    const maasBodyTemplate = (rowData) => {
        return teklifService.formatCurrency(rowData.maas);
    };

    const tarihBodyTemplate = (rowData) => {
        return teklifService.formatDate(rowData.gecerlilikTarihi);
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info p-button-sm"
                    onClick={() => showDetayDialog(rowData)}
                    tooltip="Detay"
                />
                {rowData.durum === 'Beklemede' && (
                    <>
                        <Button
                            icon="pi pi-pencil"
                            className="p-button-rounded p-button-success p-button-sm"
                            onClick={() => editTeklif(rowData)}
                            tooltip="Düzenle"
                        />
                        <Button
                            icon="pi pi-send"
                            className="p-button-rounded p-button-warning p-button-sm"
                            onClick={() => gonderTeklif(rowData)}
                            tooltip="Gönder"
                        />
                        <Button
                            icon="pi pi-trash"
                            className="p-button-rounded p-button-danger p-button-sm"
                            onClick={() => confirmDeleteTeklif(rowData)}
                            tooltip="Sil"
                        />
                    </>
                )}
                {rowData.durum === 'Gönderildi' && (
                    <Button
                        icon="pi pi-comment"
                        className="p-button-rounded p-button-help p-button-sm"
                        onClick={() => showYanitDialog(rowData)}
                        tooltip="Aday Yanıtı"
                    />
                )}
            </div>
        );
    };

    const dialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveTeklif} />
        </div>
    );

    const yanitFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveAdayYaniti} />
        </div>
    );

    return (
        <div className="card">
            <Toast ref={toast} />
            <ConfirmDialog />

            <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate} />

            <DataTable
                value={teklifler}
                loading={loading}
                dataKey="id"
                paginator
                rows={10}
                rowsPerPageOptions={[5, 10, 25]}
                globalFilter={globalFilter}
                header="Teklif Mektupları"
                emptyMessage="Teklif mektubu bulunamadı."
                currentPageReportTemplate="Toplam {totalRecords} kayıttan {first} - {last} arası gösteriliyor"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            >
                <Column field="adayAdi" header="Aday" sortable />
                <Column field="ilanBasligi" header="İlan" sortable />
                <Column field="pozisyon" header="Pozisyon" sortable />
                <Column field="maas" header="Maaş" body={maasBodyTemplate} sortable />
                <Column field="gecerlilikTarihi" header="Geçerlilik Tarihi" body={tarihBodyTemplate} sortable />
                <Column field="durum" header="Durum" body={durumBodyTemplate} sortable />
                <Column field="hazirlayanAdi" header="Hazırlayan" sortable />
                <Column body={actionBodyTemplate} header="İşlemler" style={{ minWidth: '200px' }} />
            </DataTable>

            <Dialog
                visible={teklifDialog}
                style={{ width: '750px' }}
                header="Teklif Mektubu Detayları"
                modal
                className="p-fluid"
                footer={dialogFooter}
                onHide={hideDialog}
            >
                <div className="grid">
                    <div className="col-12">
                        <div className="field">
                            <label htmlFor="basvuru">Başvuru</label>
                            <Dropdown
                                id="basvuru"
                                value={teklif.basvuruId}
                                options={basvurular}
                                onChange={(e) => setTeklif({ ...teklif, basvuruId: e.value })}
                                optionLabel="displayText"
                                optionValue="id"
                                placeholder="Başvuru seçiniz"
                                className={submitted && !teklif.basvuruId ? 'p-invalid' : ''}
                            />
                            {submitted && !teklif.basvuruId && <small className="p-error">Başvuru seçimi zorunludur.</small>}
                        </div>
                    </div>

                    <div className="col-12 md:col-6">
                        <div className="field">
                            <label htmlFor="pozisyon">Pozisyon</label>
                            <InputText
                                id="pozisyon"
                                value={teklif.pozisyon || ''}
                                onChange={(e) => setTeklif({ ...teklif, pozisyon: e.target.value })}
                                className={submitted && !teklif.pozisyon?.trim() ? 'p-invalid' : ''}
                            />
                            {submitted && !teklif.pozisyon?.trim() && <small className="p-error">Pozisyon zorunludur.</small>}
                        </div>
                    </div>

                    <div className="col-12 md:col-6">
                        <div className="field">
                            <label htmlFor="maas">Maaş (TL)</label>
                            <InputNumber
                                id="maas"
                                value={teklif.maas}
                                onValueChange={(e) => setTeklif({ ...teklif, maas: e.value })}
                                mode="currency"
                                currency="TRY"
                                locale="tr"
                                className={submitted && (!teklif.maas || teklif.maas <= 0) ? 'p-invalid' : ''}
                            />
                            {submitted && (!teklif.maas || teklif.maas <= 0) && <small className="p-error">Geçerli bir maaş giriniz.</small>}
                        </div>
                    </div>

                    <div className="col-12">
                        <div className="field">
                            <label htmlFor="ekOdemeler">Ek Ödemeler</label>
                            <InputTextarea
                                id="ekOdemeler"
                                value={teklif.ekOdemeler || ''}
                                onChange={(e) => setTeklif({ ...teklif, ekOdemeler: e.target.value })}
                                rows={3}
                            />
                        </div>
                    </div>

                    <div className="col-12 md:col-4">
                        <div className="field">
                            <label htmlFor="izinHakki">İzin Hakkı (Gün)</label>
                            <InputNumber
                                id="izinHakki"
                                value={teklif.izinHakki}
                                onValueChange={(e) => setTeklif({ ...teklif, izinHakki: e.value })}
                                min={0}
                                max={365}
                            />
                        </div>
                    </div>

                    <div className="col-12 md:col-4">
                        <div className="field">
                            <label htmlFor="iseBaslamaTarihi">İşe Başlama Tarihi</label>
                            <Calendar
                                id="iseBaslamaTarihi"
                                value={teklif.iseBaslamaTarihi}
                                onChange={(e) => setTeklif({ ...teklif, iseBaslamaTarihi: e.value })}
                                dateFormat="dd/mm/yy"
                                placeholder="gg/aa/yyyy"
                                showIcon
                                locale="tr"
                            />
                        </div>
                    </div>

                    <div className="col-12 md:col-4">
                        <div className="field">
                            <label htmlFor="gecerlilikTarihi">Geçerlilik Tarihi</label>
                            <Calendar
                                id="gecerlilikTarihi"
                                value={teklif.gecerlilikTarihi}
                                onChange={(e) => setTeklif({ ...teklif, gecerlilikTarihi: e.value })}
                                dateFormat="dd/mm/yy"
                                placeholder="gg/aa/yyyy"
                                showIcon
                                locale="tr"
                            />
                        </div>
                    </div>
                </div>
            </Dialog>

            <Dialog
                visible={yanitDialog}
                style={{ width: '500px' }}
                header="Aday Yanıtı"
                modal
                className="p-fluid"
                footer={yanitFooter}
                onHide={hideDialog}
            >
                <div className="field">
                    <label htmlFor="adayYaniti">Aday Yanıtı</label>
                    <Dropdown
                        id="adayYaniti"
                        value={selectedTeklif?.adayYaniti}
                        options={yanitOptions}
                        onChange={(e) => setSelectedTeklif({ ...selectedTeklif, adayYaniti: e.value })}
                        placeholder="Yanıt seçiniz"
                    />
                </div>

                {selectedTeklif?.adayYaniti === 'Reddetti' && (
                    <div className="field">
                        <label htmlFor="redNedeni">Red Nedeni</label>
                        <InputTextarea
                            id="redNedeni"
                            value={selectedTeklif?.redNedeni || ''}
                            onChange={(e) => setSelectedTeklif({ ...selectedTeklif, redNedeni: e.target.value })}
                            rows={3}
                        />
                    </div>
                )}
            </Dialog>

            <Dialog
                visible={detayDialog}
                style={{ width: '800px' }}
                header="Teklif Mektubu Detayı"
                modal
                onHide={hideDialog}
            >
                {selectedTeklif && (
                    <div className="grid">
                        <div className="col-12">
                            <Card>
                                <div className="grid">
                                    <div className="col-12 md:col-6">
                                        <strong>Aday:</strong> {selectedTeklif.adayAdi}
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>İlan:</strong> {selectedTeklif.ilanBasligi}
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>Pozisyon:</strong> {selectedTeklif.pozisyon}
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>Maaş:</strong> {teklifService.formatCurrency(selectedTeklif.maas)}
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>İzin Hakkı:</strong> {selectedTeklif.izinHakki} gün
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>Durum:</strong> <Badge value={selectedTeklif.durum} severity={teklifService.getDurumSeviyesi(selectedTeklif.durum)} />
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>İşe Başlama:</strong> {teklifService.formatDate(selectedTeklif.iseBaslamaTarihi)}
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>Geçerlilik:</strong> {teklifService.formatDate(selectedTeklif.gecerlilikTarihi)}
                                    </div>
                                    {selectedTeklif.ekOdemeler && (
                                        <div className="col-12">
                                            <strong>Ek Ödemeler:</strong>
                                            <p>{selectedTeklif.ekOdemeler}</p>
                                        </div>
                                    )}
                                    {selectedTeklif.redNedeni && (
                                        <div className="col-12">
                                            <strong>Red Nedeni:</strong>
                                            <p>{selectedTeklif.redNedeni}</p>
                                        </div>
                                    )}
                                    <div className="col-12 md:col-6">
                                        <strong>Hazırlayan:</strong> {selectedTeklif.hazirlayanAdi}
                                    </div>
                                    <div className="col-12 md:col-6">
                                        <strong>Oluşturma:</strong> {teklifService.formatDateTime(selectedTeklif.createdAt)}
                                    </div>
                                </div>
                            </Card>
                        </div>
                    </div>
                )}
            </Dialog>

            <Dialog
                visible={istatistikDialog}
                style={{ width: '600px' }}
                header="Teklif İstatistikleri"
                modal
                onHide={hideDialog}
            >
                <div className="grid">
                    <div className="col-6">
                        <Card>
                            <h3 className="text-center">{istatistikler.toplamTeklif}</h3>
                            <p className="text-center text-gray-600">Toplam Teklif</p>
                        </Card>
                    </div>
                    <div className="col-6">
                        <Card>
                            <h3 className="text-center">{istatistikler.beklemedeTeklif}</h3>
                            <p className="text-center text-gray-600">Beklemede</p>
                        </Card>
                    </div>
                    <div className="col-6">
                        <Card>
                            <h3 className="text-center">{istatistikler.gonderilmisTeklif}</h3>
                            <p className="text-center text-gray-600">Gönderilmiş</p>
                        </Card>
                    </div>
                    <div className="col-6">
                        <Card>
                            <h3 className="text-center">{istatistikler.kabulEdilenTeklif}</h3>
                            <p className="text-center text-gray-600">Kabul Edildi</p>
                        </Card>
                    </div>
                    <div className="col-6">
                        <Card>
                            <h3 className="text-center">%{istatistikler.kabulOrani}</h3>
                            <p className="text-center text-gray-600">Kabul Oranı</p>
                        </Card>
                    </div>
                    <div className="col-6">
                        <Card>
                            <h3 className="text-center">%{istatistikler.redOrani}</h3>
                            <p className="text-center text-gray-600">Red Oranı</p>
                        </Card>
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default TeklifYonetimi;