import { useEffect, useMemo, useRef, useState } from "react";
import { IPlant } from "../../interfaces/IPlant";
import { IPest } from "../../interfaces/IPest";
import { ISoil } from "../../interfaces/ISoil";
import { getApi, postApi, putApi, deleteApi } from "../../api";
import { Modal } from "../components/Modal";
import { PlantForm } from "./components/PlantForm";
import { CommentsSection } from "./components/CommentsSection";
import { formStyle } from "../../styles/formStyle";

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

    const visiblePlants = useMemo(() => {
        return plants.filter(p =>
            matchesFilter(p.pests, filterPest) &&
            matchesFilter(p.soilType, filterSoil) &&
            matchesQuery(p, query)
        );
    }, [plants, filterPest, filterSoil, query]);

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
                <button
                    className={formStyle.button}
                    onClick={addHandler}
                >
                   Pridėti augalą
                </button>

              
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

              
                <div className="flex-none w-40 sm:w-46">
                    <input
                        type="text"
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        placeholder="Paieška..."
                        className={formStyle.input}
                    />
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
                                                    className="h-14 w-14 rounded-md object-cover ring-1 ring-gray-200 bg-gray-50"
                                                />
                                            ) : (
                                                <div className="h-14 w-14 rounded-md ring-1 ring-gray-200 bg-gray-50 flex items-center justify-center text-[10px] text-gray-400">
                                                    Nėra
                                                </div>
                                            )}
                                            <div>
                                                <div className="font-medium text-gray-900">{plant.name}</div>
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
                                        <button
                                            className="text-emerald-700 hover:text-emerald-900 font-medium"
                                            onClick={() => editHandler(plant)}
                                        >
                                            Redaguoti
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            <CommentsSection />
        </div>
    );
}