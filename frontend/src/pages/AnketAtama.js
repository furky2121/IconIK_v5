import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { Toast } from 'primereact/toast';
import { Tag } from 'primereact/tag';
import { RadioButton } from 'primereact/radiobutton';
import anketService from '../services/anketService';
import personelService from '../services/personelService';
import departmanService from '../services/departmanService';
import pozisyonService from '../services/pozisyonService';
import yetkiService from '../services/yetkiService';
import notificationService from '../services/notificationService';
import { NotificationService } from '../services/notificationService';
import { useRouter, useSearchParams } from 'next/navigation';

const AnketAtama = () => {
    const [anket, setAnket] = useState(null);
    const [atamalar, setAtamalar] = useState([]);
    const [atamaDialog, setAtamaDialog] = useState(false);
    const [atamaTipi, setAtamaTipi] = useState('personel'); // personel, departman, pozisyon
    const [selectedPersonel, setSelectedPersonel] = useState(null);
    const [selectedDepartman, setSelectedDepartman] = useState(null);
    const [selectedPozisyon, setSelectedPozisyon] = useState(null);
    const [not, setNot] = useState('');
    const [personelList, setPersonelList] = useState([]);
    const [departmanList, setDepartmanList] = useState([]);
    const [pozisyonList, setPozisyonList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [permissions, setPermissions] = useState({ write: false });
    const toast = useRef(null);
    const router = useRouter();
    const searchParams = useSearchParams();
    const anketId = searchParams.get('id');

    useEffect(() => {
        if (anketId) {
            loadAnket();
            loadAtamalar();
            loadPermissions();
        }
    }, [anketId]);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                write: yetkiService.hasScreenPermission('anketler', 'write')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
        }
    };

    const loadAnket = async () => {
        try {
            const response = await anketService.getAnketById(anketId);
            if (response.success) {
                setAnket(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Anket yüklenirken hata oluştu',
                life: 3000
            });
        }
    };

    const loadAtamalar = async () => {
        setLoading(true);
        try {
            const response = await anketService.getAnketAtamalari(anketId);
            if (response.success) {
                setAtamalar(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Atamalar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const loadDropdowns = async () => {
        try {
            const [personelResp, departmanResp, pozisyonResp] = await Promise.all([
                personelService.getPersonellerAktif(),
                departmanService.getAktifDepartmanlar(),
                pozisyonService.getAktifPozisyonlar()
            ]);

            if (personelResp.success) {
                setPersonelList(personelResp.data.map(p => ({
                    label: `${p.ad} ${p.soyad}`,
                    value: p.id
                })));
            }

            if (departmanResp.success) {
                setDepartmanList(departmanResp.data.map(d => ({
                    label: d.ad,
                    value: d.id
                })));
            }

            if (pozisyonResp.success) {
                setPozisyonList(pozisyonResp.data.map(p => ({
                    label: p.ad,
                    value: p.id
                })));
            }
        } catch (error) {
            // console.error('Dropdown loading error:', error);
        }
    };

    const createNotifications = async (atamaData) => {
        try {
            let targetPersonelIds = [];

            if (atamaData.personelId) {
                // Doğrudan personele atandıysa
                targetPersonelIds = [atamaData.personelId];
            } else if (atamaData.departmanId) {
                // Departmana atandıysa, o departmandaki tüm personeli al
                const personelResp = await personelService.getPersonellerAktif();
                if (personelResp.success) {
                    targetPersonelIds = personelResp.data
                        .filter(p => p.pozisyon?.departmanId === atamaData.departmanId)
                        .map(p => p.id);
                }
            } else if (atamaData.pozisyonId) {
                // Pozisyona atandıysa, o pozisyondaki tüm personeli al
                const personelResp = await personelService.getPersonellerAktif();
                if (personelResp.success) {
                    targetPersonelIds = personelResp.data
                        .filter(p => p.pozisyonId === atamaData.pozisyonId)
                        .map(p => p.id);
                }
            }

            // Her bir personel için bildirim oluştur
            for (const targetPersonelId of targetPersonelIds) {
                await notificationService.addNotification({
                    personelId: targetPersonelId,
                    baslik: 'Yeni Anket Ataması',
                    mesaj: `Size "${anket?.baslik || 'Bir anket'}" anketi atandı.`,
                    kategori: NotificationService.CATEGORIES.ANKET,
                    iliskiliId: atamaData.anketId,
                    oncelik: 'normal',
                    link: `/bana-atanan-anketler`
                });
            }
        } catch (error) {
            // console.error('Bildirim oluşturma hatası:', error);
            // Bildirim hatası ana işlemi etkilememeli
        }
    };

    const yeniAtama = () => {
        loadDropdowns();
        setAtamaTipi('personel');
        setSelectedPersonel(null);
        setSelectedDepartman(null);
        setSelectedPozisyon(null);
        setNot('');
        setAtamaDialog(true);
    };

    const kaydetAtama = async () => {
        const userStr = localStorage.getItem('user');
        if (!userStr) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Oturum bilgisi bulunamadı. Lütfen tekrar giriş yapın.',
                life: 3000
            });
            return;
        }

        const user = JSON.parse(userStr);
        const personelId = user?.personel?.id;

        if (!personelId) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Kullanıcı bilgisi hatalı. Lütfen tekrar giriş yapın.',
                life: 3000
            });
            return;
        }

        const atamaData = {
            anketId: parseInt(anketId),
            personelId: atamaTipi === 'personel' ? selectedPersonel : null,
            departmanId: atamaTipi === 'departman' ? selectedDepartman : null,
            pozisyonId: atamaTipi === 'pozisyon' ? selectedPozisyon : null,
            atayanPersonelId: personelId,
            not: not
        };

        try {
            const response = await anketService.createAtama(atamaData);
            if (response.success) {
                // Bildirimleri oluştur
                await createNotifications(atamaData);

                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Atama başarıyla oluşturuldu',
                    life: 3000
                });
                setAtamaDialog(false);
                loadAtamalar();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Atama oluşturulurken hata oluştu',
                life: 3000
            });
        }
    };

    const atamaYiSil = async (atamaId) => {
        if (!confirm('Bu atamayı silmek istediğinizden emin misiniz?')) return;

        try {
            const response = await anketService.deleteAtama(atamaId);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Atama silindi',
                    life: 3000
                });
                loadAtamalar();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Atama silinirken hata oluştu',
                life: 3000
            });
        }
    };

    const atamaTipiTemplate = (rowData) => {
        if (rowData.personel) {
            return <Tag value="Personel" severity="success" />;
        } else if (rowData.departman) {
            return <Tag value="Departman" severity="info" />;
        } else if (rowData.pozisyon) {
            return <Tag value="Pozisyon" severity="warning" />;
        }
        return '-';
    };

    const atamaHedefTemplate = (rowData) => {
        if (rowData.personel) {
            return `${rowData.personel.ad} ${rowData.personel.soyad}`;
        } else if (rowData.departman) {
            return rowData.departman.ad;
        } else if (rowData.pozisyon) {
            return rowData.pozisyon.ad;
        }
        return '-';
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <Button
                icon="pi pi-trash"
                rounded
                outlined
                severity="danger"
                onClick={() => atamaYiSil(rowData.id)}
                disabled={!permissions.write}
            />
        );
    };

    if (!anketId) {
        return (
            <Card>
                <div className="text-center">
                    <i className="pi pi-exclamation-triangle" style={{ fontSize: '3rem', color: 'var(--orange-500)' }}></i>
                    <h3>Geçersiz İstek</h3>
                    <p>Anket ID'si bulunamadı.</p>
                    <Button label="Anketlere Dön" icon="pi pi-arrow-left" onClick={() => router.push('/anketler')} />
                </div>
            </Card>
        );
    }

    if (!permissions.write) {
        return (
            <div className="flex align-items-center justify-content-center" style={{ minHeight: '400px' }}>
                <Card>
                    <div className="text-center">
                        <i className="pi pi-lock" style={{ fontSize: '3rem', color: 'var(--primary-color)' }}></i>
                        <h3>Yetkiniz Yok</h3>
                        <p>Anket atama yapmak için yetkiniz bulunmamaktadır.</p>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <Card title="Anket Atama">
                    <Toast ref={toast} />

                    {anket && (
                        <div className="mb-4 p-3 surface-50 border-round">
                            <h5>Anket Bilgileri</h5>
                            <p><strong>Başlık:</strong> {anket.baslik}</p>
                            <p><strong>Açıklama:</strong> {anket.aciklama || '-'}</p>
                            <p>
                                <strong>Süre:</strong> {anketService.formatDate(anket.baslangicTarihi)} - {anketService.formatDate(anket.bitisTarihi)}
                            </p>
                        </div>
                    )}

                    <div className="mb-3">
                        <Button
                            label="Yeni Atama"
                            icon="pi pi-plus"
                            onClick={yeniAtama}
                            disabled={!permissions.write}
                        />
                    </div>

                    <DataTable
                        value={atamalar}
                        loading={loading}
                        emptyMessage="Atama bulunamadı"
                        responsiveLayout="scroll"
                    >
                        <Column field="id" header="ID" style={{ width: '5rem' }} />
                        <Column body={atamaTipiTemplate} header="Atama Tipi" style={{ width: '10rem' }} />
                        <Column body={atamaHedefTemplate} header="Atanan" />
                        <Column field="atayanPersonel.ad" header="Atayan" />
                        <Column field="not" header="Not" />
                        <Column body={actionBodyTemplate} header="İşlemler" style={{ width: '8rem' }} />
                    </DataTable>
                </Card>
            </div>

            <Dialog
                visible={atamaDialog}
                style={{ width: '500px' }}
                header="Yeni Atama"
                modal
                onHide={() => setAtamaDialog(false)}
            >
                <div className="p-fluid">
                    <div className="field">
                        <label>Atama Tipi</label>
                        <div className="flex gap-3 mt-2">
                            <div className="flex align-items-center">
                                <RadioButton
                                    inputId="tip-personel"
                                    value="personel"
                                    onChange={(e) => setAtamaTipi(e.value)}
                                    checked={atamaTipi === 'personel'}
                                />
                                <label htmlFor="tip-personel" className="ml-2">Personel</label>
                            </div>
                            <div className="flex align-items-center">
                                <RadioButton
                                    inputId="tip-departman"
                                    value="departman"
                                    onChange={(e) => setAtamaTipi(e.value)}
                                    checked={atamaTipi === 'departman'}
                                />
                                <label htmlFor="tip-departman" className="ml-2">Departman</label>
                            </div>
                            <div className="flex align-items-center">
                                <RadioButton
                                    inputId="tip-pozisyon"
                                    value="pozisyon"
                                    onChange={(e) => setAtamaTipi(e.value)}
                                    checked={atamaTipi === 'pozisyon'}
                                />
                                <label htmlFor="tip-pozisyon" className="ml-2">Pozisyon</label>
                            </div>
                        </div>
                    </div>

                    {atamaTipi === 'personel' && (
                        <div className="field">
                            <label htmlFor="personel">Personel</label>
                            <Dropdown
                                id="personel"
                                value={selectedPersonel}
                                options={personelList}
                                onChange={(e) => setSelectedPersonel(e.value)}
                                placeholder="Personel seçiniz"
                                filter
                            />
                        </div>
                    )}

                    {atamaTipi === 'departman' && (
                        <div className="field">
                            <label htmlFor="departman">Departman</label>
                            <Dropdown
                                id="departman"
                                value={selectedDepartman}
                                options={departmanList}
                                onChange={(e) => setSelectedDepartman(e.value)}
                                placeholder="Departman seçiniz"
                                filter
                            />
                        </div>
                    )}

                    {atamaTipi === 'pozisyon' && (
                        <div className="field">
                            <label htmlFor="pozisyon">Pozisyon</label>
                            <Dropdown
                                id="pozisyon"
                                value={selectedPozisyon}
                                options={pozisyonList}
                                onChange={(e) => setSelectedPozisyon(e.value)}
                                placeholder="Pozisyon seçiniz"
                                filter
                            />
                        </div>
                    )}

                    <div className="field">
                        <label htmlFor="not">Not</label>
                        <InputTextarea
                            id="not"
                            value={not}
                            onChange={(e) => setNot(e.target.value)}
                            rows={3}
                        />
                    </div>
                </div>

                <div className="flex justify-content-end gap-2 mt-3">
                    <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={() => setAtamaDialog(false)} />
                    <Button label="Kaydet" icon="pi pi-check" onClick={kaydetAtama} />
                </div>
            </Dialog>
        </div>
    );
};

export default AnketAtama;
