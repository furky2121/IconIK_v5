import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { Checkbox } from 'primereact/checkbox';
import { Toast } from 'primereact/toast';
import { Card } from 'primereact/card';
import { TabView, TabPanel } from 'primereact/tabview';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { Badge } from 'primereact/badge';
import { Message } from 'primereact/message';
import yetkiService from '../services/yetkiService';
import kademeService from '../services/kademeService';

const Ayarlar = () => {
    const [kademeYetkileri, setKademeYetkileri] = useState([]);
    const [ekranYetkileri, setEkranYetkileri] = useState([]);
    const [kademeler, setKademeler] = useState([]);
    const [loading, setLoading] = useState(false);
    const [editDialogVisible, setEditDialogVisible] = useState(false);
    const [selectedYetki, setSelectedYetki] = useState(null);
    const [activeIndex, setActiveIndex] = useState(0);
    const toast = useRef(null);

    useEffect(() => {
        loadData();
        
        // Check if user object has proper kademe.id structure
        const checkUserStructure = () => {
            const user = JSON.parse(localStorage.getItem('user') || '{}');
            if (user && user.personel && user.personel.pozisyon && user.personel.pozisyon.kademe) {
                // Check if kademe has id property
                if (!user.personel.pozisyon.kademe.id) {
            // console.log('User object missing kademe.id, clearing localStorage...');
                    localStorage.clear();
                    window.location.href = '/login';
                    return false;
                }
            }
            return true;
        };
        
        if (checkUserStructure()) {
            // Load user permissions without the problematic API call
            // since the page will show all permissions anyway
            const initializePermissions = async () => {
                try {
                    // Try to load permissions but don't fail if it doesn't work
                    await yetkiService.loadUserPermissions();
                } catch (error) {
            // console.log('Could not load user permissions in Ayarlar page:', error.message);
                }
            };
            
            initializePermissions();
        }
    }, []);

    const loadData = async () => {
        setLoading(true);
        try {
            const [yetkilerRes, ekranlarRes, kademelerRes] = await Promise.all([
                yetkiService.getKademeYetkileri(),
                yetkiService.getEkranYetkileri(),
                kademeService.getAllKademeler()
            ]);

            setKademeYetkileri(yetkilerRes || []);
            setEkranYetkileri(ekranlarRes || []);
            setKademeler(kademelerRes?.data || []);
        } catch (error) {
            // console.error('LoadData Error:', error);
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Veriler yüklenirken hata oluştu: ' + error.message
            });
        } finally {
            setLoading(false);
        }
    };

    const editYetki = (yetki) => {
        setSelectedYetki({...yetki});
        setEditDialogVisible(true);
    };

    const saveYetki = async () => {
        try {
            await yetkiService.updateKademeYetkisi(selectedYetki.id, selectedYetki);
            
            toast.current?.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'Yetki başarıyla güncellendi'
            });

            setEditDialogVisible(false);
            loadData();
        } catch (error) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Yetki güncellenirken hata oluştu: ' + error.message
            });
        }
    };

    const deleteYetki = (yetki) => {
        confirmDialog({
            message: `${yetki.kademeAdi} - ${yetki.ekranAdi} yetkisini silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: async () => {
                try {
                    await yetkiService.deleteKademeYetkisi(yetki.id);
                    
                    toast.current?.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: 'Yetki başarıyla silindi'
                    });

                    loadData();
                } catch (error) {
                    toast.current?.show({
                        severity: 'error',
                        summary: 'Hata',
                        detail: 'Yetki silinirken hata oluştu: ' + error.message
                    });
                }
            }
        });
    };

    const createDefaultPermissions = async () => {
        confirmDialog({
            message: 'Varsayılan ekran yetkileri ve kademe yetkileri oluşturulacak. Devam etmek istiyor musunuz?',
            header: 'Varsayılan Yetkiler',
            icon: 'pi pi-question-circle',
            accept: async () => {
                try {
                    setLoading(true);
                    
                    // First create screen permissions
                    try {
                        await yetkiService.createDefaultEkranYetkileri();
                        toast.current?.show({
                            severity: 'info',
                            summary: 'Bilgi',
                            detail: 'Ekran yetkileri oluşturuldu'
                        });
                    } catch (screenError) {
            // console.log('Screen permissions might already exist:', screenError.message);
                    }
                    
                    // Then create kademe permissions
                    try {
                        await yetkiService.createDefaultKademeYetkileri();
                        toast.current?.show({
                            severity: 'info',
                            summary: 'Bilgi',
                            detail: 'Kademe yetkileri oluşturuldu'
                        });
                    } catch (kademeError) {
            // console.log('Kademe permissions creation error:', kademeError.message);
                    }
                    
                    toast.current?.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: 'Varsayılan yetkiler işlemi tamamlandı'
                    });

                    await loadData();
                } catch (error) {
            // console.error('Default permissions error:', error);
                    toast.current?.show({
                        severity: 'error',
                        summary: 'Hata',
                        detail: 'Varsayılan yetkiler oluşturulurken hata: ' + error.message
                    });
                } finally {
                    setLoading(false);
                }
            }
        });
    };

    // Table columns
    const kademeBodyTemplate = (rowData) => {
        return (
            <div className="flex align-items-center gap-2">
                <Badge value={rowData.kademeSeviye} severity="info" />
                <span>{rowData.kademeAdi}</span>
            </div>
        );
    };

    const yetkilerBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                {rowData.okumaYetkisi && <Badge value="Okuma" severity="success" />}
                {rowData.yazmaYetkisi && <Badge value="Yazma" severity="warning" />}
                {rowData.guncellemeYetkisi && <Badge value="Güncelleme" severity="info" />}
                {rowData.silmeYetkisi && <Badge value="Silme" severity="danger" />}
            </div>
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-pencil"
                    size="small"
                    className="p-button-rounded p-button-warning"
                    onClick={() => editYetki(rowData)}
                    tooltip="Düzenle"
                />
                <Button
                    icon="pi pi-trash"
                    size="small"
                    className="p-button-rounded p-button-danger"
                    onClick={() => deleteYetki(rowData)}
                    tooltip="Sil"
                />
            </div>
        );
    };

    const dialogFooter = (
        <div>
            <Button 
                label="İptal" 
                icon="pi pi-times" 
                onClick={() => setEditDialogVisible(false)} 
                className="p-button-text" 
            />
            <Button 
                label="Kaydet" 
                icon="pi pi-check" 
                onClick={saveYetki} 
                autoFocus 
            />
        </div>
    );

    return (
        <div className="grid">
            <Toast ref={toast} />
            <ConfirmDialog />
            
            <div className="col-12">
                <Card title="Sistem Ayarları" className="shadow-3">
                    <div className="mb-4">
                        <Button
                            label="Varsayılan Yetkileri Oluştur"
                            icon="pi pi-plus"
                            onClick={createDefaultPermissions}
                            loading={loading}
                            className="p-button-success"
                            tooltip="Tüm ekranlar ve kademeler için varsayılan yetki matrisini oluşturur"
                        />
                    </div>

                    <TabView activeIndex={activeIndex} onTabChange={(e) => setActiveIndex(e.index)}>
                        <TabPanel header="İzin Hiyerarşisi" leftIcon="pi pi-calendar">
                            <div className="mb-4">
                                <p className="text-600">
                                    Bu bölümde izin onay hiyerarşisini yönetebilirsiniz. Her kademe seviyesi hangi personellerin izin taleplerini onaylayabilir belirleyebilirsiniz.
                                </p>
                                <div className="grid">
                                    <div className="col-12">
                                        <div className="surface-card p-4 shadow-2 border-round">
                                            <h6>İzin Onay Hiyerarşisi Kuralları</h6>
                                            <ul className="list-none p-0 m-0">
                                                <li className="flex align-items-start mb-3">
                                                    <Badge value="1" severity="info" className="mr-2 mt-1" />
                                                    <div>
                                                        <strong>Genel Müdür:</strong> Tüm personellerin izin taleplerini görüntüleyebilir ve onaylayabilir.
                                                    </div>
                                                </li>
                                                <li className="flex align-items-start mb-3">
                                                    <Badge value="2" severity="info" className="mr-2 mt-1" />
                                                    <div>
                                                        <strong>Genel Müdür Yardımcısı:</strong> Kendi departmanındaki tüm personellerin izin taleplerini görüntüleyebilir ve onaylayabilir.
                                                    </div>
                                                </li>
                                                <li className="flex align-items-start mb-3">
                                                    <Badge value="3-6" severity="info" className="mr-2 mt-1" />
                                                    <div>
                                                        <strong>Diğer Kademeler (Direktör, Grup Müdürü, Müdür, Yönetici, Şef):</strong> Sadece kendilerine bağlı olan personellerin izin taleplerini görüntüleyebilir ve onaylayabilir.
                                                    </div>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                    <div className="col-12">
                                        <div className="surface-card p-4 shadow-2 border-round">
                                            <h6>Güvenlik ve Yetki Notu</h6>
                                            <Message 
                                                severity="warn" 
                                                text="Bu hiyerarşi otomatik olarak uygulanmaktadır. Personeller sadece yetkili oldukları izin taleplerini görebilir ve onaylayabilirler." 
                                                className="w-full"
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </TabPanel>
                        <TabPanel header="Kademe Yetkileri" leftIcon="pi pi-users">
                            <div className="mb-4">
                                <p className="text-600">
                                    Bu tabloda her kademe seviyesinin hangi ekranlarda ne tür yetkilerinin olduğunu görüntüleyebilir ve düzenleyebilirsiniz.
                                </p>
                            </div>
                            
                            <DataTable
                                value={kademeYetkileri}
                                paginator
                                rows={10}
                                loading={loading}
                                emptyMessage="Henüz yetki tanımlaması yapılmamış"
                                className="p-datatable-striped"
                                sortMode="multiple"
                                removableSort
                            >
                                <Column field="kademeAdi" header="Kademe" body={kademeBodyTemplate} sortable />
                                <Column field="ekranAdi" header="Ekran" sortable />
                                <Column field="ekranKodu" header="Kod" sortable />
                                <Column header="Yetkiler" body={yetkilerBodyTemplate} />
                                <Column header="İşlemler" body={actionBodyTemplate} style={{ width: '120px' }} />
                            </DataTable>
                        </TabPanel>

                        <TabPanel header="Ekran Tanımları" leftIcon="pi pi-desktop">
                            <div className="mb-4">
                                <p className="text-600">
                                    Sistemdeki tüm ekranlar ve modüller bu listede görüntülenir.
                                </p>
                            </div>
                            
                            <DataTable
                                value={ekranYetkileri}
                                paginator
                                rows={10}
                                loading={loading}
                                emptyMessage="Henüz ekran tanımlaması yapılmamış"
                                className="p-datatable-striped"
                            >
                                <Column field="ekranAdi" header="Ekran Adı" sortable />
                                <Column field="ekranKodu" header="Kod" sortable />
                                <Column field="aciklama" header="Açıklama" />
                                <Column 
                                    field="aktif" 
                                    header="Durum" 
                                    body={(rowData) => (
                                        <Badge 
                                            value={rowData.aktif ? 'Aktif' : 'Pasif'} 
                                            severity={rowData.aktif ? 'success' : 'danger'} 
                                        />
                                    )}
                                />
                            </DataTable>
                        </TabPanel>
                    </TabView>
                </Card>
            </div>

            {/* Edit Dialog */}
            <Dialog
                header="Yetki Düzenle"
                visible={editDialogVisible}
                style={{ width: '450px' }}
                footer={dialogFooter}
                modal
                onHide={() => setEditDialogVisible(false)}
            >
                {selectedYetki && (
                    <div>
                        <div className="mb-4">
                            <h5 className="m-0">{selectedYetki.kademeAdi} - {selectedYetki.ekranAdi}</h5>
                            <small className="text-600">Kademe Seviye: {selectedYetki.kademeSeviye} | Ekran Kodu: {selectedYetki.ekranKodu}</small>
                        </div>

                        <div className="flex flex-column gap-3">
                            <div className="field-checkbox">
                                <Checkbox
                                    inputId="okuma"
                                    checked={selectedYetki.okumaYetkisi}
                                    onChange={(e) => setSelectedYetki({...selectedYetki, okumaYetkisi: e.checked})}
                                />
                                <label htmlFor="okuma" className="ml-2">Okuma Yetkisi</label>
                            </div>

                            <div className="field-checkbox">
                                <Checkbox
                                    inputId="yazma"
                                    checked={selectedYetki.yazmaYetkisi}
                                    onChange={(e) => setSelectedYetki({...selectedYetki, yazmaYetkisi: e.checked})}
                                />
                                <label htmlFor="yazma" className="ml-2">Yazma Yetkisi</label>
                            </div>

                            <div className="field-checkbox">
                                <Checkbox
                                    inputId="guncelleme"
                                    checked={selectedYetki.guncellemeYetkisi}
                                    onChange={(e) => setSelectedYetki({...selectedYetki, guncellemeYetkisi: e.checked})}
                                />
                                <label htmlFor="guncelleme" className="ml-2">Güncelleme Yetkisi</label>
                            </div>

                            <div className="field-checkbox">
                                <Checkbox
                                    inputId="silme"
                                    checked={selectedYetki.silmeYetkisi}
                                    onChange={(e) => setSelectedYetki({...selectedYetki, silmeYetkisi: e.checked})}
                                />
                                <label htmlFor="silme" className="ml-2">Silme Yetkisi</label>
                            </div>
                        </div>
                    </div>
                )}
            </Dialog>
        </div>
    );
};

export default Ayarlar;