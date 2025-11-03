import React from "react";
import { Link, Outlet, useFetchers, useNavigation } from "react-router-dom";
import { useUiStore } from "../store/uiStore"; 

const FLOWER_URL =
  "data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><circle cx='50' cy='50' r='10' fill='%23facc15'/><circle cx='50' cy='25' r='18' fill='%23f472b6'/><circle cx='75' cy='45' r='18' fill='%23f472b6'/><circle cx='65' cy='75' r='18' fill='%23f472b6'/><circle cx='35' cy='75' r='18' fill='%23f472b6'/><circle cx='25' cy='45' r='18' fill='%23f472b6'/></svg>";

export function Layout() {
    const navigation = useNavigation();
    const fetchers = useFetchers();
    const fetcherInProgress = fetchers.some((f) => ["Loading", "submitting"].includes(f.state));

    const { isSidebarOpen, toggleSidebar, closeSidebar } = useUiStore(); 

    return <div className='container mx-auto flex flex-col gap-y-6'>
        <header className='relative bg-green-600 text-white p-3 mb-0 overflow-hidden'>
            <img src={FLOWER_URL} alt="" aria-hidden="true"
                 className="absolute left-4 top-1/2 -translate-y-1/2 h-20 opacity-90 z-0 pointer-events-none select-none" />

            <div className="relative z-10">
                <div className='flex items-center gap-3'>
                    <div className='text-3xl'>LINO SODYBA</div>
                   
                </div>

                <nav className="mt-1">
                    <div className="flex items-center">
                        <ul className='flex gap-x-3'>
                            <li><Link to="/" onClick={closeSidebar}>Pradžia</Link></li>
                            <li><Link to="/plants" onClick={closeSidebar}>Augalai</Link></li>
                            <li><Link to="/pests" onClick={closeSidebar}>Kenkėjai</Link></li>
                            <li><Link to="/soils" onClick={closeSidebar}>Dirvožemiai</Link></li>
                        </ul>
                        <ul className='flex gap-x-3 ml-auto'>
                            <li><Link to="/register" onClick={closeSidebar}>Registruotis</Link></li>
                            <li><Link to="/login" onClick={closeSidebar}>Prisijungti</Link></li>
                        </ul>
                    </div>
                </nav>

            </div>
        </header>

        <div>
            {navigation.state !== "idle" && <div className="m-1">Navigaion in progress...</div>}
            {fetcherInProgress && <div className="m-1"> Fetcher in progress...</div>}
        </div>

        <Outlet />

        <footer className='bg-gray-500 text-white-sm flex content-center justify-center items-center h-10'>
            <div>Panevėžio kolegija</div>
        </footer>
    </div>
}