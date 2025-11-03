import { useEffect, useState } from "react";
import { ISoil } from "../../interfaces/ISoil";
import { getApi, postApi, putApi, deleteApi } from "../../api";
import { Modal } from "../components/Modal";
import { SoilForm } from "./components/SoilForm";

export default function Soils() {
    const [soils, setSoils] = useState<ISoil[]>([]);
    const [visibleModal, setVisibleModal] = useState<boolean>(false);
    const [editSoil, setEditSoil] = useState<ISoil | undefined>();

    const getSoils = () =>
        getApi<ISoil[]>("soils").then((s) => s && setSoils(s));

    const storeSoil = (soil: ISoil) => {
        setVisibleModal(false);

        if (soil.id) {
            putApi(`soils/${soil.id}`, soil).then(() => getSoils());
        } else {
            postApi("soils", soil).then(() => getSoils());
        }
    };

    const deleteSoil = (id: number | undefined) => {
        if (!id) return;
        setVisibleModal(false);
        deleteApi(`soils/${id}`, {}).then(() => getSoils());
    };

    const editHandler = (soil: ISoil) => {
        setEditSoil(soil);
        setVisibleModal(true);
    };

    const addHandler = () => {
        setEditSoil(undefined);
        setVisibleModal(true);
    };

    useEffect(() => {
        getSoils();
    }, []);

    return (
        <div>
            {visibleModal && (
                <Modal
                    visibleModal={visibleModal}
                    setVisibleModal={setVisibleModal}
                    title="Dirvožemio forma"
                >
                    <SoilForm
                        storeSoil={storeSoil}
                        soil={editSoil}
                        deleteSoil={deleteSoil}
                    />
                </Modal>
            )}

            <div className="flex items-center gap-3 mb-4">
                <div className="text-3xl">Dirvožemiai</div>
                <button
                    className="bg-green-600 text-white px-4 py-2 rounded"
                    onClick={addHandler}
                >
                    Pridėti dirvožemį
                </button>
            </div>

            <table className="min-w-full border border-green-800">
                <thead>
                    <tr className="bg-gray-100">
                        <th className="border border-green-800 px-4 py-2 text-left">Pavadinimas</th>
                        <th className="border border-green-800 px-4 py-2"></th>
                    </tr>
                </thead>
                <tbody>
                    {soils.map((soil) => (
                        <tr key={soil.id} className="border border-green-800">
                            <td className="border border-green-800 px-4 py-2 align-top">
                                <div>{soil.name}</div>
                            </td>
                            <td className="border border-green-800 px-4 py-2">
                                <button
                                    className="underline text-blue-600"
                                    onClick={() => editHandler(soil)}
                                >
                                    Edit
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}