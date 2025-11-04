import { useEffect, useState } from "react";
import { IPest } from "../../interfaces/IPest";
import { getApi, postApi, putApi, deleteApi } from "../../api";
import { Modal } from "../components/Modal";
import { PestForm } from "./components/PestForm";
import { formStyle } from "../../styles/formStyle";
import { useAuthStore } from "@/store/authStore";

export default function Pests() {
    const [pests, setPests] = useState<IPest[]>([]);
    const [visibleModal, setVisibleModal] = useState<boolean>(false);
    const [editPest, setEditPest] = useState<IPest | undefined>();

    const isAdmin = useAuthStore(s => s.isAdmin);

    const getPests = () =>
        getApi<IPest[]>("pests").then((p) => p && setPests(p));

    const storePest = (pest: IPest) => {
        setVisibleModal(false);

        if (pest.id) {
            putApi(`pests/${pest.id}`, pest).then(() => getPests());
        } else {
            postApi("pests", pest).then(() => getPests());
        }
    };

    const deletePest = (id: number | undefined) => {
        if (!id) return;
        setVisibleModal(false);
        deleteApi(`pests/${id}`, {}).then(() => getPests());
    };

    const editHandler = (pest: IPest) => {
        setEditPest(pest);
        setVisibleModal(true);
    };

    const addHandler = () => {
        setEditPest(undefined);
        setVisibleModal(true);
    };

    useEffect(() => {
        getPests();
    }, []);

    return (
        <div>
            {visibleModal && (
                <Modal
                    visibleModal={visibleModal}
                    setVisibleModal={setVisibleModal}
                    title="Kenkėjo forma"
                >
                    <PestForm
                        storePest={storePest}
                        pest={editPest}
                        deletePest={deletePest}
                    />
                </Modal>
            )}

            <div className="flex items-center gap-3 mb-4">
                <div className="text-3xl">Kenkėjai</div>
                {isAdmin() && (
                    <button
                        className={formStyle.button}
                        onClick={addHandler}
                    >
                        Pridėti kenkėją
                    </button>
                )}
            </div>

            <table className="min-w-full border border-[#065f46] border-separate border-spacing-0">
                <thead>
                    <tr className="bg-gray-100">
                        <th className="border border-[#065f46] px-4 py-2 text-left">Pavadinimas</th>
                        <th className="border border-[#065f46] px-4 py-2"></th>
                    </tr>
                </thead>
                <tbody>
                    {pests.map((pest) => (
                        <tr key={pest.id} className="border border-[#065f46]">
                            <td className="border border-[#065f46] px-4 py-2 align-top">
                                <div>{pest.name}</div>
                                {pest.imageUrl && (
                                    <img
                                        src={pest.imageUrl}
                                        alt={pest.name}
                                        className="h-12 mt-1"
                                    />
                                )}
                            </td>
                            <td className="border border-[#065f46] px-4 py-2">
                                {isAdmin() && (
                                    <button
                                        className="underline text-blue-600"
                                        onClick={() => editHandler(pest)}
                                    >
                                        Redaguoti
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}