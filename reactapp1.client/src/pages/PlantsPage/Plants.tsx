import { useEffect, useMemo, useRef, useState } from "react";
import { IPlant } from "../../interfaces/IPlant";
import { IPest } from "../../interfaces/IPest";
import { ISoil } from "../../interfaces/ISoil";
import { getApi, postApi, putApi, deleteApi } from "../../api";
import { Modal } from "../components/Modal";
import { PlantForm } from "./components/PlantForm";
import { CommentsSection } from "./components/CommentsSection";
import { formStyle } from "../../styles/formStyle";
import { useAuthStore } from "@/store/authStore";

export default function Plants() {
    const [plants, setPlants] = useState<IPlant[]>([]);
    const [visibleModal, setVisibleModal] = useState<boolean>(false);
    const [editPlant, setEditPlant] = useState<IPlant | undefined>();

    const [allPests, setAllPests] = useState<IPest[]>([]);
    const [allSoils, setAllSoils] = useState<ISoil[]>([]);
    const [filterPest, setFilterPest] = useState<string>("");
    const [filterSoil, setFilterSoil] = useState<string>("");
    const [showPestFilter, setShowPestFilter] = useState(false);
    const [showSoilFilter, setShowSoilFilter] = useState(false);

    const pestFilterRef = useRef<HTMLDivElement>(null);
    const soilFilterRef = useRef<HTMLDivElement>(null);
    const searchInputRef = useRef<HTMLInputElement>(null);

    const user = useAuthStore(s => s.user);
    const isAdmin = useAuthStore(s => s.isAdmin);
    const isSignedIn = !!user;

    // User "My plants" (per-user) + filter
    const [savedIds, setSavedIds] = useState<Set<number>>(new Set());
    const [onlyMine, setOnlyMine] = useState<boolean>(false);

    // Inline lightbox state (no extra files)
    const [lightbox, setLightbox] = useState<{ src: string; alt: string } | null>(null);

    // Keyed by user id/email to separate per-user
    const favKey = user ? `myplants:${user.id || user.email}` : null;

    const loadSaved = () => {
        if (!favKey) { setSavedIds(new Set()); return; }
        try {
            const raw = localStorage.getItem(favKey);
            const arr = raw ? (JSON.parse(raw) as number[]) : [];
            setSavedIds(new Set(arr.filter(n => typeof n === "number")));
        } catch {
            setSavedIds(new Set());
        }
    };
    const persistSaved = (next: Set<number>) => {
        if (!favKey) return;
        try {
            localStorage.setItem(favKey, JSON.stringify(Array.from(next)));
        } catch { /* ignore */ }
    };

    const addMy = (id?: number) => {
        if (!id) return;
        setSavedIds(prev => {
            const next = new Set(prev);
            next.add(id);
            persistSaved(next);
            return next;
        });
    };
    const removeMy = (id?: number) => {
        if (!id) return;
        setSavedIds(prev => {
            const next = new Set(prev);
            next.delete(id);
            persistSaved(next);
            return next;
        });
    };

    useEffect(() => { loadSaved(); /* reload when user changes */ }, [favKey]);

    useEffect(() => {
        setShowPestFilter(false);
        setShowSoilFilter(false);

        const onDocMouseDown = (e: MouseEvent) => {
            const target = e.target as Node;
            if (pestFilterRef.current && !pestFilterRef.current.contains(target)) {
                setShowPestFilter(false);
            }
            if (soilFilterRef.current && !soilFilterRef.current.contains(target)) {
                setShowSoilFilter(false);
            }
        };
        document.addEventListener("mousedown", onDocMouseDown);
        return () => document.removeEventListener("mousedown", onDocMouseDown);
    }, []);

    const getPlants = () =>
        getApi<IPlant[]>("plants").then((p) => p && setPlants(p));

    const storePlant = (plant: IPlant) => {
        setVisibleModal(false);

        if (plant.id) {
            putApi(`plants/${plant.id}`, plant).then(() => getPlants());
        } else {
            postApi("plants", plant).then(() => getPlants());
        }
    };

    const deletePlant = (id: number | undefined) => {
        if (!id) return;
        setVisibleModal(false);
        deleteApi(`plants/${id}`, {}).then(() => getPlants());
    };

    const editHandler = (plant: IPlant) => {
        setEditPlant(plant);
        setVisibleModal(true);
    };

    const addHandler = () => {
        setEditPlant(undefined);
        setVisibleModal(true);
    };

    useEffect(() => {
        getPlants();
        getApi<IPest[]>("pests").then(p => p && setAllPests(p));
        getApi<ISoil[]>("soils").then(s => s && setAllSoils(s));
    }, []);

    const matchesFilter = (csv: string | undefined, selected: string) => {
        if (!selected) return true;
        if (!csv) return false;
        const parts = csv
            .split(",")
            .map(s => s.trim().toLowerCase())
            .filter(Boolean);
        return parts.includes(selected.toLowerCase());
    };

    const matchesQuery = (plant: IPlant, q: string) => {
        if (!q) return true;
        const needle = q.toLowerCase();
        return (
            plant.name.toLowerCase().includes(needle) ||
            (plant.description ?? "").toLowerCase().includes(needle)
        );
    };

    const [query, setQuery] = useState<string>("");

    const inMyPlants = (p: IPlant) => (p.id ? savedIds.has(p.id) : false);

    const visiblePlants = useMemo(() => {
        return plants.filter(p =>
            matchesFilter(p.pests, filterPest) &&
            matchesFilter(p.soilType, filterSoil) &&
            matchesQuery(p, query) &&
            (!onlyMine || inMyPlants(p))
        );
    }, [plants, filterPest, filterSoil, query, onlyMine, savedIds]);

    const singlePlant = visiblePlants.length === 1 ? visiblePlants[0] : undefined;

    const insertNameToSearch = (name: string) => {
        setQuery(name);
        searchInputRef.current?.focus();
    };

    return (
        <div>
            {visibleModal && (
                <Modal
                    visibleModal={visibleModal}
                    setVisibleModal={setVisibleModal}
                    title="Augalo forma"
                >
                    <PlantForm
                        storePlant={storePlant}
                        plant={editPlant}
                        deletePlant={deletePlant}
                    />
                </Modal>
            )}

            <div className="flex items-center gap-3 mb-4 flex-wrap">
                <div className="text-3xl">Augalai</div>

                {isAdmin() && (
                    <button
                        className={formStyle.button}
                        onClick={addHandler}
                    >
                        Pridėti augalą
                    </button>
                )}

                <div className="relative" ref={pestFilterRef}>
                    <button
                        type="button"
                        className={formStyle.button}
                        onClick={() => setShowPestFilter(v => !v)}
                    >
                        Filtruoti kenkėjus
                    </button>
                    {showPestFilter && (
                        <div className="absolute z-10 mt-2 bg-white border rounded shadow p-3 min-w-60">
                            <label className="block text-sm mb-1">Kenkėjai</label>
                            <select
                                className={formStyle.input}
                                value={filterPest}
                                onChange={(e) => setFilterPest(e.target.value)}
                            >
                                <option value="">-- Visi --</option>
                                {allPests.map(p => (
                                    <option key={p.id} value={p.name}>{p.name}</option>
                                ))}
                            </select>
                            <div className="flex justify-between mt-2">
                                <button
                                    type="button"
                                    className="underline text-sm"
                                    onClick={() => setFilterPest("")}
                                >
                                    Išvalyti
                                </button>
                                <button
                                    type="button"
                                    className="underline text-sm"
                                    onClick={() => setShowPestFilter(false)}
                                >
                                    Uždaryti
                                </button>
                            </div>
                        </div>
                    )}
                </div>

                <div className="relative" ref={soilFilterRef}>
                    <button
                        type="button"
                        className={formStyle.button}
                        onClick={() => setShowSoilFilter(v => !v)}
                    >
                        Filtruoti dirvožemį
                    </button>
                    {showSoilFilter && (
                        <div className="absolute z-10 mt-2 bg-white border rounded shadow p-3 min-w-60">
                            <label className="block text-sm mb-1">Dirvožemio tipas</label>
                            <select
                                className={formStyle.input}
                                value={filterSoil}
                                onChange={(e) => setFilterSoil(e.target.value)}
                            >
                                <option value="">-- Visi --</option>
                                {allSoils.map(s => (
                                    <option key={s.id} value={s.name}>{s.name}</option>
                                ))}
                            </select>
                            <div className="flex justify-between mt-2">
                                <button
                                    type="button"
                                    className="underline text-sm"
                                    onClick={() => setFilterSoil("")}
                                >
                                    Išvalyti
                                </button>
                                <button
                                    type="button"
                                    className="underline text-sm"
                                    onClick={() => setShowSoilFilter(false)}
                                >
                                    Uždaryti
                                </button>
                            </div>
                        </div>
                    )}
                </div>

                {/* Right-aligned: search + "Mano augalai" */}
                <div className="ml-auto flex items-center gap-3">
                    {/* Search with inline clear button */}
                    <div className="relative flex-none w-40 sm:w-46">
                        <input
                            ref={searchInputRef}
                            type="text"
                            value={query}
                            onChange={(e) => setQuery(e.target.value)}
                            placeholder="Paieška..."
                            className={`${formStyle.input} pr-9`}
                        />
                        {query && (
                            <button
                                type="button"
                                aria-label="Išvalyti paiešką"
                                className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                                onClick={() => { setQuery(""); searchInputRef.current?.focus(); }}
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" className="h-4 w-4">
                                    <path fillRule="evenodd" d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm-1.28-5.03 1.28-1.28 1.28 1.28a.75.75 0 1 0 1.06-1.06L11.06 9.59l1.28-1.28A.75.75 0 1 0 11.28 7.25L10 8.53 8.72 7.25a.75.75 0 1 0-1.06 1.06l1.28 1.28-1.28 1.28a.75.75 0 1 0 1.06 1.06Z" clipRule="evenodd" />
                                </svg>
                            </button>
                        )}
                    </div>

                    {isSignedIn && (
                        <button
                            type="button"
                            className={`${formStyle.button} ${onlyMine ? "!bg-emerald-700 hover:!bg-emerald-800" : ""}`}
                            title="Rodyti tik mano išsaugotus augalus"
                            onClick={() => setOnlyMine(v => !v)}
                        >
                            {onlyMine ? "Mano augalai (įjungta)" : "Mano augalai"}
                        </button>
                    )}
                </div>
            </div>

            <div className="overflow-x-auto">
                <div className="overflow-hidden rounded-lg shadow ring-1 ring-gray-200">
                    <table className="min-w-full table-auto">
                        <thead>
                            <tr className="bg-gray-50">
                                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Pavadinimas</th>
                                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Aprašymas</th>
                                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Žemės tipas</th>
                                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Kenkėjai</th>
                                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Kenkėjų kontrolė</th>
                                <th className="px-4 py-3" />
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100 bg-white">
                            {visiblePlants.map((plant) => (
                                <tr key={plant.id} className="hover:bg-gray-50">
                                    <td className="px-4 py-3 align-top">
                                        <div className="flex items-start gap-3">
                                            {plant.imageUrl ? (
                                                <img
                                                    src={plant.imageUrl}
                                                    alt={plant.name}
                                                    className="h-14 w-14 rounded-md object-cover ring-1 ring-gray-200 bg-gray-50 cursor-zoom-in"
                                                    onClick={() => setLightbox({ src: plant.imageUrl!, alt: plant.name })}
                                                />
                                            ) : (
                                                <div className="h-14 w-14 rounded-md ring-1 ring-gray-200 bg-gray-50 flex items-center justify-center text-[10px] text-gray-400">
                                                    Nėra
                                                </div>
                                            )}
                                            <div>
                                                <button
                                                    type="button"
                                                    className="font-medium text-gray-900 hover:text-emerald-700 hover:underline cursor-pointer"
                                                    title="Ieškoti pagal šį pavadinimą"
                                                    onClick={() => insertNameToSearch(plant.name)}
                                                >
                                                    {plant.name}
                                                </button>
                                            </div>
                                        </div>
                                    </td>

                                    <td className="px-4 py-3 align-top">
                                        <div className="text-sm text-gray-700">
                                            {plant.description ?? "-"}
                                        </div>
                                    </td>

                                    <td className="px-4 py-3 align-top">
                                        {plant.soilType ? (
                                            <span className="inline-flex items-center rounded-md bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-700 ring-1 ring-inset ring-amber-600/20">
                                                {plant.soilType}
                                            </span>
                                        ) : (
                                            <span className="text-sm text-gray-400">-</span>
                                        )}
                                    </td>

                                    <td className="px-4 py-3 align-top">
                                        {plant.pests ? (
                                            <div className="flex flex-wrap gap-1">
                                                {plant.pests
                                                    .split(",")
                                                    .map(p => p.trim())
                                                    .filter(Boolean)
                                                    .map((p, idx) => (
                                                        <span
                                                            key={`${p}-${idx}`}
                                                            className="inline-flex items-center rounded-md bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700 ring-1 ring-inset ring-emerald-600/20"
                                                        >
                                                            {p}
                                                        </span>
                                                    ))}
                                            </div>
                                        ) : (
                                            <span className="text-sm text-gray-400">-</span>
                                        )}
                                    </td>

                                    <td className="px-4 py-3 align-top">
                                        <div className="text-sm text-gray-700">
                                            {plant.pestControlMethod ?? "-"}
                                        </div>
                                    </td>

                                    <td className="px-4 py-3 align-top text-right">
                                        {isAdmin() ? (
                                            <button
                                                type="button"
                                                className={formStyle.button}
                                                onClick={() => editHandler(plant)}
                                            >
                                                Redaguoti
                                            </button>
                                        ) : isSignedIn ? (
                                            savedIds.has(plant.id ?? -1) ? (
                                                <button
                                                    type="button"
                                                    className={formStyle.button}
                                                    onClick={() => removeMy(plant.id)}
                                                >
                                                    Šalinti
                                                </button>
                                            ) : (
                                                <button
                                                    type="button"
                                                    className={formStyle.button}
                                                    onClick={() => addMy(plant.id)}
                                                >
                                                    Auginu
                                                </button>
                                            )
                                        ) : null}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Inline lightbox overlay (click outside to close) */}
            {lightbox && (
                <div
                    className="fixed inset-0 z-[1000] flex items-center justify-center"
                    onClick={() => setLightbox(null)}
                    aria-modal="true"
                    role="dialog"
                >
                    <div className="absolute inset-0 bg-black/75" />
                    <div className="relative z-10 p-3" onClick={(e) => e.stopPropagation()}>
                        <img
                            src={lightbox.src}
                            alt={lightbox.alt}
                            className="max-h-[90vh] max-w-[90vw] rounded-lg shadow-2xl ring-1 ring-white/20"
                        />
                        <div className="mt-2 text-center text-sm text-white/90 drop-shadow">
                            {lightbox.alt}
                        </div>
                    </div>
                </div>
            )}

            {/* Show comments only when exactly one plant is found */}
            {singlePlant && (
                <CommentsSection plantId={singlePlant.id!} plantName={singlePlant.name} />
            )}
        </div>
    );
}