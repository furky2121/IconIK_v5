import dynamic from 'next/dynamic';

// Dynamic import to prevent SSR issues with Dashboard
const Dashboard = dynamic(() => import('../../../src/pages/Dashboard'), {
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

export default function DashboardPage() {
    return <Dashboard />;
}