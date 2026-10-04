package com.emckeon97.projectclark

import android.content.Context

/** Tiny SharedPreferences holder: best score + selected character. */
object Prefs {
    private const val FILE = "projectclark"
    private const val KEY_BEST = "clark.best"
    private const val KEY_SELECTED = "clark.selected"

    fun best(context: Context): Int =
        prefs(context).getInt(KEY_BEST, 0)

    fun setBest(context: Context, v: Int) =
        prefs(context).edit().putInt(KEY_BEST, v).apply()

    fun selected(context: Context): String =
        prefs(context).getString(KEY_SELECTED, "popeye") ?: "popeye"

    fun setSelected(context: Context, id: String) =
        prefs(context).edit().putString(KEY_SELECTED, id).apply()

    private fun prefs(context: Context) =
        context.getSharedPreferences(FILE, Context.MODE_PRIVATE)
}
