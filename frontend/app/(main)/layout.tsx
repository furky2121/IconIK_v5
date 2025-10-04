import { Metadata, Viewport } from 'next';
import Layout from '../../layout/layout';

interface AppLayoutProps {
    children: React.ReactNode;
}

// Force dynamic rendering to prevent Calendar locale errors during build
// This prevents static generation of pages with Calendar components
export const dynamic = 'force-dynamic';

// Separate viewport from metadata (required when using dynamic config)
export const viewport: Viewport = {
    initialScale: 1,
    width: 'device-width'
};

export const metadata: Metadata = {
    title: 'IK Yönetim Uygulaması',
    description: 'The ultimate collection of design-agnostic, flexible and accessible React UI Components.',
    robots: { index: false, follow: false },
    openGraph: {
        type: 'website',
        title: 'IK Yönetim Uygulaması',
        url: 'https://sakai.primereact.org/',
        description: 'The ultimate collection of design-agnostic, flexible and accessible React UI Components.',
        images: ['https://www.primefaces.org/static/social/sakai-react.png'],
        ttl: 604800
    },
    icons: {
        icon: '/favicon.ico'
    }
};

export default function AppLayout({ children }: AppLayoutProps) {
    return <Layout>{children}</Layout>;
}
