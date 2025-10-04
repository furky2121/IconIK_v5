'use client';

import dynamic from 'next/dynamic';

// Dynamic import to prevent SSR issues
const Profil = dynamic(() => import('../../../src/pages/Profil'), {
    ssr: false,
    loading: () => (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '100vh',
            fontSize: '1.2rem',
            color: '#666'
        }}>
            Yükleniyor...
        </div>
    )
});

const ProfilPage = () => {
    return <Profil />;
};

export default ProfilPage;